using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorAdminPanel.Services
{
    public class NavManagerHistory
    {
        public NavManagerHistory(NavigationManager navigationManager, ILogger<NavManagerHistory> logger)
        {
            NavigationManager = navigationManager;
            _logger = logger;
            _historyStack = new Stack<string>(100);
            _historyStack.TrimExcess();
            NavigationManager.LocationChanged += OnLocationChanged;

            NavigationManager.NavigateTo(NavigationManager.Uri, false, true);
        }
        public NavigationManager NavigationManager { get; set; }
        private Stack<string> _historyStack;
        private ILogger<NavManagerHistory> _logger;

        private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
        {
            if(args.IsNavigationIntercepted) return;

            if (_historyStack.Count == 0)
            {
                _historyStack.Push(args.Location);
               //   _logger.LogInformation($"NavManagerHistory add [{args.Location}]");
            }
            else if (_historyStack.TryPeek(out string peek) && peek != args.Location)
            {
                _historyStack.Push(args.Location);
              //    _logger.LogInformation($"NavManagerHistory add [{args.Location}]");
            }

            //_logger.LogInformation($"NavManagerHistory count [{_historyStack.Count}]");
        }

        public void GoBack()
        {
            if (_historyStack.Count == 1)
            {
                NavigationManager.NavigateTo(_historyStack.Pop());
            }
            else if (_historyStack.Count >= 2)
            {
                string url1 = _historyStack.Pop();
                //  _logger.LogInformation($"NavManagerHistory remove [{url1}]");
                string url2 = _historyStack.Pop();
                //  _logger.LogInformation($"NavManagerHistory remove [{url2}]");
                NavigationManager.NavigateTo(url2);
            }

            //_logger.LogInformation($"NavManagerHistory count [{_historyStack.Count}]");
        }

        public bool HasBack => _historyStack.Count > 0;
    }
}
