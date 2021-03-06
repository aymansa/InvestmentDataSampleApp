﻿using System;

using Xamarin.Forms;

using InvestmentDataSampleApp.Shared;

namespace InvestmentDataSampleApp
{
    public class OpportunitiesPage : BaseContentPage<OpportunitiesViewModel>
    {
        #region Constant Fields
        readonly RelativeLayout _mainLayout;
        readonly SearchBar _searchBar;
        #endregion

        #region Fields
        ListView _listView;
        ToolbarItem _addButtonToolBar;
        WelcomeView _welcomeView;
        #endregion

        #region Constructors
        public OpportunitiesPage()
        {
            #region Create the ListView
            _listView = new ListView
            {
                ItemTemplate = new DataTemplate(typeof(OpportunitiesViewCell)),
                RowHeight = 75
            };
            _listView.IsPullToRefreshEnabled = true;
            _listView.SetBinding(ListView.ItemsSourceProperty, nameof(ViewModel.ViewableOpportunitiesData));
            _listView.SetBinding(ListView.RefreshCommandProperty, nameof(ViewModel.RefreshAllDataCommand));
            #endregion

            #region Initialize the Toolbar Add Button
            _addButtonToolBar = new ToolbarItem
            {
                Icon = "Add",
                AutomationId = AutomationIdConstants.AddOpportunityButton
            };
            ToolbarItems.Add(_addButtonToolBar);
            #endregion

            #region Create Searchbar
            _searchBar = new SearchBar
            {
                AutomationId = AutomationIdConstants.OpportunitySearchBar
            };
            _searchBar.SetBinding(SearchBar.TextProperty, nameof(ViewModel.SearchBarText));
            #endregion

            _mainLayout = new RelativeLayout();

            Func<RelativeLayout, double> getSearchBarHeight = (p) => _searchBar.Measure(p.Width, p.Height).Request.Height;

            _mainLayout.Children.Add(_searchBar,
                Constraint.Constant(0),
                Constraint.Constant(0),
                 Constraint.RelativeToParent(parent => parent.Width)
            );
            _mainLayout.Children.Add(_listView,
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => getSearchBarHeight(parent)),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height - getSearchBarHeight(parent))
               );

            Title = PageTitleConstants.OpportunitiesPageTitle;

            NavigationPage.SetBackButtonTitle(this, "");

            Content = _mainLayout;

            DisplayWelcomeView();
        }

        #endregion

        #region Methods
        protected override void OnAppearing()
        {
            base.OnAppearing();

            _listView.BeginRefresh();
        }

        protected override void SubscribeEventHandlers()
        {
            ViewModel.PullToRefreshDataCompleted += HandlePullToRefreshDataCompleted;
            ViewModel.OkButtonTapped += HandleWelcomeViewDisappearing;
            _listView.ItemSelected += HandleListViewItemSelected;
            _addButtonToolBar.Clicked += HandleAddButtonClicked;
        }

        protected override void UnsubscribeEventHandlers()
        {
            ViewModel.PullToRefreshDataCompleted -= HandlePullToRefreshDataCompleted;
            ViewModel.OkButtonTapped -= HandleWelcomeViewDisappearing;
            _listView.ItemSelected -= HandleListViewItemSelected;
            _addButtonToolBar.Clicked -= HandleAddButtonClicked;
        }

        void HandleListViewItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var itemSelected = e?.SelectedItem as OpportunityModel;

                await Navigation?.PushAsync(new OpportunityDetailsPage(itemSelected));

				_listView.SelectedItem = null;

                _listView.EndRefresh();
            });
        }

        void HandlePullToRefreshDataCompleted(object sender, EventArgs e)=>
            Device.BeginInvokeOnMainThread(_listView.EndRefresh);

        async void HandleAddButtonClicked(object sender, EventArgs e) =>
            await Navigation?.PushModalAsync(new NavigationPage(new AddOpportunityPage()));

        void HandleWelcomeViewDisappearing(object sender, EventArgs e) => _welcomeView?.HideView();

        void DisplayWelcomeView()
        {
            if (!(Settings.ShouldShowWelcomeView))
                return;

            Device.BeginInvokeOnMainThread(() =>
            {
                _welcomeView = new WelcomeView();

                _mainLayout?.Children?.Add(_welcomeView,
                   Constraint.Constant(0),
                   Constraint.Constant(0));

                _welcomeView?.ShowView(true);
            });
        }
        #endregion
    }
}

