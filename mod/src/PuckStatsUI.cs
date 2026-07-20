using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace PuckStats
{
    /// <summary>
    /// PuckStats UI overlay built with UI Toolkit.
    /// Displays real-time analytics: speed, distance, ratings, session info.
    /// Designed to look like a native game UI element.
    /// </summary>
    public static class PuckStatsUI
    {
        private static VisualElement _panel;
        private static VisualElement _mainContainer;
        private static Label _speedLabel;
        private static Label _distanceLabel;
        private static Label _ratingLabel;
        private static Label _matchTimeLabel;
        private static Label _sessionLabel;
        private static bool _visible;
        private static bool _initialized;
        private static float _currentSpeed;
        private static float _totalDistance;
        private static float _matchTime;
        private static string _currentRating = "--";

        // Style constants matching game aesthetic
        private static readonly Color PanelBg = new(0.08f, 0.08f, 0.1f, 0.92f);
        private static readonly Color HeaderBg = new(0.12f, 0.12f, 0.18f, 1f);
        private static readonly Color AccentColor = new(0.26f, 1f, 0.56f, 1f); // #42ff8f
        private static readonly Color TextColor = new(0.9f, 0.9f, 0.92f, 1f);
        private static readonly Color MutedColor = new(0.63f, 0.63f, 0.63f, 1f);
        private static readonly Color BorderColor = new(0.13f, 0.13f, 0.13f, 1f);

        public static bool IsOpen => _visible;

        public static void Initialize()
        {
            if (_initialized) return;
            try
            {
                var uiDoc = UIManager.Instance?.UIDocument;
                if (uiDoc == null) return;

                BuildPanel(uiDoc.rootVisualElement);
                _initialized = true;
                Plugin.Log("PuckStats UI initialized");
            }
            catch (System.Exception e)
            {
                Plugin.LogError($"UI Init failed: {e}");
            }
        }

        private static void BuildPanel(VisualElement root)
        {
            _panel = new VisualElement();
            _panel.name = "PuckStatsPanel";
            _panel.style.position = Position.Absolute;
            _panel.style.right = 12;
            _panel.style.top = 80;
            _panel.style.width = 240;
            _panel.style.backgroundColor = PanelBg;
            _panel.style.borderTopLeftRadius = 8;
            _panel.style.borderTopRightRadius = 8;
            _panel.style.borderBottomLeftRadius = 8;
            _panel.style.borderBottomRightRadius = 8;
            _panel.style.borderLeftWidth = 1;
            _panel.style.borderRightWidth = 1;
            _panel.style.borderTopWidth = 1;
            _panel.style.borderBottomWidth = 1;
            _panel.style.borderLeftColor = BorderColor;
            _panel.style.borderRightColor = BorderColor;
            _panel.style.borderTopColor = BorderColor;
            _panel.style.borderBottomColor = BorderColor;
            _panel.style.display = DisplayStyle.None;
            _panel.pickingMode = PickingMode.Ignore;

            // Header
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.paddingLeft = 12;
            header.style.paddingRight = 12;
            header.style.paddingTop = 8;
            header.style.paddingBottom = 8;
            header.style.backgroundColor = HeaderBg;
            header.style.borderTopLeftRadius = 8;
            header.style.borderTopRightRadius = 8;

            var title = new Label("PUCKSTATS");
            title.style.color = AccentColor;
            title.style.fontSize = 11;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.letterSpacing = 1.5f;
            header.Add(title);

            var versionLabel = new Label("v1.0");
            versionLabel.style.color = MutedColor;
            versionLabel.style.fontSize = 9;
            header.Add(versionLabel);
            _panel.Add(header);

            // Divider
            _panel.Add(MakeDivider());

            // Content container
            _mainContainer = new VisualElement();
            _mainContainer.style.paddingLeft = 12;
            _mainContainer.style.paddingRight = 12;
            _mainContainer.style.paddingTop = 6;
            _mainContainer.style.paddingBottom = 6;

            // Speed row
            _mainContainer.Add(MakeStatRow("Speed", out _speedLabel, "0.0 m/s"));

            // Distance row
            _mainContainer.Add(MakeStatRow("Distance", out _distanceLabel, "0m"));

            // Match time
            _mainContainer.Add(MakeStatRow("Match Time", out _matchTimeLabel, "00:00"));

            // Rating
            var ratingRow = new VisualElement();
            ratingRow.style.flexDirection = FlexDirection.Row;
            ratingRow.style.justifyContent = Justify.SpaceBetween;
            ratingRow.style.alignItems = Align.Center;
            ratingRow.style.height = 22;

            var ratingTitle = new Label("Rating");
            ratingTitle.style.color = MutedColor;
            ratingTitle.style.fontSize = 10;
            ratingRow.Add(ratingTitle);

            _ratingLabel = new Label("--");
            _ratingLabel.style.color = AccentColor;
            _ratingLabel.style.fontSize = 12;
            _ratingLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            ratingRow.Add(_ratingLabel);
            _mainContainer.Add(ratingRow);

            _panel.Add(_mainContainer);
            _panel.Add(MakeDivider());

            // Footer with session
            var footer = new VisualElement();
            footer.style.paddingLeft = 12;
            footer.style.paddingRight = 12;
            footer.style.paddingTop = 4;
            footer.style.paddingBottom = 6;
            _sessionLabel = new Label("Session: --");
            _sessionLabel.style.color = MutedColor;
            _sessionLabel.style.fontSize = 9;
            footer.Add(_sessionLabel);
            _panel.Add(footer);

            root.Add(_panel);
        }

        private static VisualElement MakeStatRow(string name, out Label valueLabel, string defaultValue)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.alignItems = Align.Center;
            row.style.height = 22;
            row.style.marginBottom = 2;

            var nameLabel = new Label(name);
            nameLabel.style.color = MutedColor;
            nameLabel.style.fontSize = 10;
            row.Add(nameLabel);

            valueLabel = new Label(defaultValue);
            valueLabel.style.color = TextColor;
            valueLabel.style.fontSize = 11;
            valueLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(valueLabel);

            return row;
        }

        private static VisualElement MakeDivider()
        {
            var div = new VisualElement();
            div.style.height = 1;
            div.style.backgroundColor = BorderColor;
            div.style.marginLeft = 0;
            div.style.marginRight = 0;
            return div;
        }

        public static void Tick()
        {
            if (_panel == null || !_visible) return;

            // Hotkey: Ctrl+P to toggle
            if (Keyboard.current?.ctrlKey.isPressed == true &&
                Keyboard.current?.pKey.wasPressedThisFrame == true)
            {
                Toggle();
            }

            // Update in real-time if in match
            try
            {
                var local = PlayerManager.Instance?.GetLocalPlayer();
                if (local?.PlayerBody != null)
                {
                    var rb = local.PlayerBody.Rigidbody;
                    _currentSpeed = rb != null ? rb.linearVelocity.magnitude : 0f;
                    _speedLabel.text = $"{_currentSpeed:F1} m/s";

                    if (_currentSpeed > 8f)
                        _speedLabel.style.color = AccentColor;
                    else if (_currentSpeed > 4f)
                        _speedLabel.style.color = TextColor;
                    else
                        _speedLabel.style.color = MutedColor;
                }
            }
            catch { }
        }

        public static void ShowInMatch()
        {
            if (_panel == null) return;
            _panel.style.display = DisplayStyle.Flex;
            _visible = true;
            _matchTime = 0f;
            _totalDistance = 0f;
            PlatformRebind();
        }

        public static void ShowOutOfMatch()
        {
            if (_panel == null) return;
            _visible = false;
            _panel.style.display = DisplayStyle.None;
        }

        public static void UpdateMatchTime(float time)
        {
            _matchTime = time;
            int mins = Mathf.FloorToInt(time / 60f);
            int secs = Mathf.FloorToInt(time % 60f);
            if (_matchTimeLabel != null)
                _matchTimeLabel.text = $"{mins:00}:{secs:00}";
        }

        public static void UpdateDistance(float distance)
        {
            _totalDistance = distance;
            if (_distanceLabel != null)
                _distanceLabel.text = distance > 1000 ? $"{distance / 1000:F1}km" : $"{distance:F0}m";
        }

        public static void UpdateRating(int rating)
        {
            _currentRating = rating.ToString();
            if (_ratingLabel != null)
            {
                _ratingLabel.text = _currentRating;
                if (rating >= 80) _ratingLabel.style.color = AccentColor;
                else if (rating >= 50) _ratingLabel.style.color = new Color(1f, 0.75f, 0.3f, 1f);
                else _ratingLabel.style.color = new Color(1f, 0.4f, 0.4f, 1f);
            }
        }

        public static void UpdateSession(string sessionInfo)
        {
            if (_sessionLabel != null)
                _sessionLabel.text = $"Session: {sessionInfo}";
        }

        public static void SetUploadStatus(string status)
        {
            Plugin.Log($"Upload status: {status}");
        }

        private static void Toggle()
        {
            if (_visible) ShowOutOfMatch();
            else ShowInMatch();
        }

        /// <summary>
        /// Re-attach panel to current UIDocument on scene change.
        /// </summary>
        private static void PlatformRebind()
        {
            if (_panel?.panel == null)
            {
                var uiDoc = UnityEngine.Object.FindFirstObjectByType<UIDocument>();
                if (uiDoc != null)
                {
                    _panel?.RemoveFromHierarchy();
                    uiDoc.rootVisualElement.Add(_panel);
                }
            }
        }

        public static void Destroy()
        {
            if (_panel?.parent != null)
                _panel.RemoveFromHierarchy();
            _panel = null;
            _initialized = false;
        }
    }
}
