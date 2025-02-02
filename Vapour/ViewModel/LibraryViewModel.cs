﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Vapour.Command;
using Vapour.Model;
using Vapour.Model.Dto;
using Vapour.State;
using Vapour.ViewModel.Base;

namespace Vapour.ViewModel
{
    public class LibraryViewModel : BaseViewModel
    {

        private readonly VapourDatabaseEntities _dataContext;
        private readonly IAuthenticator _authenticator;
        private List<GameDto> _gamesCollection = new List<GameDto>();
        private List<GameCommentDto> _comments = new List<GameCommentDto>();

        public LibraryViewModel(VapourDatabaseEntities dataContext, IAuthenticator authenticator)
        {
            _dataContext = dataContext;
            _authenticator = authenticator;
            GetUserGames();
            CheckIfLibraryIsEmpty();
        }

        private void CheckIfLibraryIsEmpty()
        {
            if (_gamesCollection.Count > 0)
            {
                SelectedGame = GamesCollection[0];
                TextVisibility = "Hidden";
                PanelVisibility = "Visible";
            }
            else
            {
                TextVisibility = "Visible";
                PanelVisibility = "Hidden";
            }
        }

        private List<GameCommentDto> GetGameComments(int id)
        {
            var comments = _dataContext.Comments.OrderByDescending(x => x.CreatedAt).ToList();
            var gameComments = new List<GameCommentDto>();
            foreach (var comment in comments)
            {
                if (comment.GameId == id)
                {
                    var user = _dataContext.Users.FirstOrDefault(u => u.Id == comment.UserId);
                    var isFollowing = "";
                    if (_dataContext.Follows
                            .Where(x => x.FollowerId == user.Id)
                            .Where(y => y.UserId == _authenticator.CurrentUser.Id)
                            .Count(z => z.FollowerId != _authenticator.CurrentUser.Id) != 0)
                    {
                        isFollowing = "(Obserwujesz)";
                    }
                    gameComments.Add(new GameCommentDto()
                    {
                        User = user.Name,
                        IsFollowing = isFollowing,
                        Id = comment.Id,
                        Text = comment.Text,
                        Date = comment.CreatedAt.ToString()
                    });
                }
            }
            return gameComments;
        }

        private void GetUserGames()
        {
            var userGames = from gs in _dataContext.GamesCollections
                join g in _dataContext.Games on gs.GameId equals g.Id
                where gs.UserId == _authenticator.CurrentUser.Id
                select new
                {
                    Title = g.Title,
                    Id = g.Id,
                };
            foreach (var game in userGames)
            {
                _gamesCollection.Add(new GameDto()
                {
                    Id = game.Id,
                    Title = game.Title,
                });
            }
        }

        private string GetAverageRate(int id)
        {
            if (_dataContext.Rates.Any(r => r.GameId == id) == false)
            {
                return "NA";
            }
            double SumRate = 0;
            int howMany = 0;
            foreach (var rate in _dataContext.Rates)
            {
                if (rate.GameId == id)
                {
                    SumRate += rate.Rate1;
                    howMany++;
                }
            }
            return Math.Round((SumRate / howMany), 2).ToString();
        }

        private GameDto _selectedGame;
        public GameDto SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                Title = value.Title;
                AverageRate = GetAverageRate(value.Id);
                Comments = GetGameComments(value.Id);
                CheckForCommentEdit();
                CheckForRateEdit();
                CommentText = "";
                OnPropertyChanged(nameof(SelectedGame));
            }
        }

        public List<GameDto> GamesCollection
        {
            get => _gamesCollection;
            set
            {
                _gamesCollection = value;
                OnPropertyChanged(nameof(GamesCollection));
            }
        }
        public List<GameCommentDto> Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged(nameof(Comments));
            }
        }

        private GameCommentDto _selectedComment;
        public GameCommentDto SelectedComment
        {
            get => _selectedComment;
            set
            {
                _selectedComment = value;
                if (value != null)
                {
                    CommentId = value.Id;
                }
                OnPropertyChanged(nameof(SelectedComment));
                CheckForCommentEdit();
            }
        }

        private string _textVisibility;
        public string TextVisibility
        {
            get => _textVisibility;
            set
            {
                _textVisibility = value;
                OnPropertyChanged(nameof(TextVisibility));
            }
        }

        private string _panelVisibility;
        public string PanelVisibility
        {
            get => _panelVisibility;
            set
            {
                _panelVisibility = value;
                OnPropertyChanged(nameof(PanelVisibility));
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _averageRate;
        public string AverageRate
        {
            get => _averageRate;
            set
            {
                _averageRate = value;
                OnPropertyChanged(nameof(AverageRate));
            }
        }

        private string _commentButtonContent;
        public string CommentButtonContent
        {
            get { return _commentButtonContent; }
            set
            {
                _commentButtonContent = value;
                OnPropertyChanged(nameof(CommentButtonContent));
            }
        }

        private string _rateButtonContent;
        public string RateButtonContent
        {
            get { return _rateButtonContent; }
            set
            {
                _rateButtonContent = value;
                OnPropertyChanged(nameof(RateButtonContent));
            }
        }

        private int _sliderValue;
        public int SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                SliderText = value.ToString();
                OnPropertyChanged(nameof(SliderValue));
            }
        }

        private string _sliderText;
        public string SliderText
        {
            get { return _sliderText; }
            set
            {
                _sliderText = value;
                OnPropertyChanged(nameof(SliderText));
            }
        }

        private string _commentText;
        public string CommentText
        {
            get { return _commentText; }
            set
            {
                _commentText = value;
                OnPropertyChanged(nameof(CommentText));
            }
        }

        private int _commentId;
        public int CommentId
        {
            get => _commentId;
            set
            {
                _commentId = value;
                OnPropertyChanged(nameof(CommentId));
            }
        }

        private int _currentRateId;
        public int CurrentRateId
        {
            get => _currentRateId;
            set
            {
                _currentRateId = value;
                OnPropertyChanged(nameof(CurrentRateId));
            }
        }

        private bool _rateEdit;
        public bool RateEdit
        {
            get => _rateEdit;
            set
            {
                _rateEdit = value;
                if (value)
                {
                    RateButtonContent = "Edytuj";
                }
                else
                {
                    RateButtonContent = "Dodaj ocene";
                    SliderValue = 1;
                }
                OnPropertyChanged(nameof(RateEdit));
            }
        }

        private bool _commentEdit;
        public bool CommentEdit
        {
            get => _commentEdit;
            set
            {
                _commentEdit = value;
                if (value)
                {
                    CommentButtonContent = "Edytuj";
                    CommentText = SelectedComment.Text;
                }
                else
                {
                    CommentButtonContent = "Dodaj nowy komentarz";
                    CommentText = "";
                }
                OnPropertyChanged(nameof(CommentEdit));
            }
        }

        private ICommand _playGame;
        public ICommand PlayGame
        {
            get
            {
                return _playGame ?? (_playGame = new RelayCommand(
                    o => { MessageBox.Show("Gra jest właśnie uruchamiana"); },
                    o => true));
            }
        }

        private ICommand _addComment;
        public ICommand AddComment
        {
            get
            {
                return _addComment ?? (_addComment = new RelayCommand(
                    o =>
                    {
                        if (SelectedComment != null && CommentEdit)
                        {
                            MessageBox.Show("Komentarz został edytowany");
                            _dataContext.Comments.AddOrUpdate(new Comment()
                            {
                                Id = SelectedComment.Id,
                                UserId = _authenticator.CurrentUser.Id,
                                GameId = SelectedGame.Id,
                                Text = CommentText,
                                CreatedAt = Convert.ToDateTime(SelectedComment.Date)
                            });
                            _dataContext.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show("Pomyślnie dodano komentarz");
                            _dataContext.Comments.Add(new Comment()
                            {
                                UserId = _authenticator.CurrentUser.Id,
                                GameId = SelectedGame.Id,
                                Text = CommentText,
                                CreatedAt = DateTime.UtcNow
                            });
                            _dataContext.SaveChanges();
                        }
                        Comments = GetGameComments(SelectedGame.Id);
                        CheckForCommentEdit();
                    },
                    (object o) => { return true; }));
            }
        }

        private ICommand _addRate;
        public ICommand AddRate
        {
            get
            {
                return _addRate ?? (_addRate = new RelayCommand(
                    o =>
                    {
                        if (CurrentRateId != null && RateEdit)
                        {
                            MessageBox.Show("Edytowano poprzednią ocenę");
                            _dataContext.Rates.AddOrUpdate(new Rate()
                            {
                                Id = CurrentRateId,
                                UserId = _authenticator.CurrentUser.Id,
                                GameId = SelectedGame.Id,
                                Rate1 = SliderValue
                            });
                            _dataContext.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show("Dodano nową ocenę: " + SliderValue);
                            _dataContext.Rates.Add(new Rate()
                            {
                                UserId = _authenticator.CurrentUser.Id,
                                GameId = SelectedGame.Id,
                                Rate1 = SliderValue
                            });
                            _dataContext.SaveChanges();
                        }
                        AverageRate = GetAverageRate(SelectedGame.Id);
                        CheckForRateEdit();
                    },
                    o => true));
            }
        }

        public bool CheckForCommentEdit()
        {
            var query = from c in _dataContext.Comments
                where c.Id == CommentId
                select new
                {
                    userId = c.UserId
                };
            try
            {
                if (query.First().userId == _authenticator.CurrentUser.Id)
                {
                    CommentEdit = true;
                    return true;
                }
            }
            catch (Exception e)
            {
                CommentEdit = false;
                return false;
            }
            CommentEdit = false;
            return false;
        }

        public bool CheckForRateEdit()
        {
            var query = from c in _dataContext.Rates
                where c.GameId == SelectedGame.Id 
                && c.UserId == _authenticator.CurrentUser.Id
                select new
                {
                    userId = c.UserId,
                    rate = c.Rate1,
                    rateId = c.Id
                };
            try
            {
                if (query.First().userId == _authenticator.CurrentUser.Id)
                {
                    RateEdit = true;
                    SliderValue = query.First().rate;
                    CurrentRateId = query.First().rateId;
                    return true;
                }
            }
            catch (Exception e)
            {
                RateEdit = false;
                return false;
            }
            RateEdit = false;
            return false;
        }
    }
}
