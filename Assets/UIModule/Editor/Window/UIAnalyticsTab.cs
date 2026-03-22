using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIModule
{
    public class UIAnalyticsTab : VisualElement
    {

        private string searchText = "";
        private int sortMode = 0;


        private UISettings settings;

        public void Init(UISettings settings)
        {
            this.settings = settings;

            var tab = CreateAnalyticTab();
            Add(tab);
        }

        private VisualElement CreateAnalyticTab()
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.marginTop = 10;
            container.style.flexDirection = FlexDirection.Column;

            // Header Section (Fixed, not scrollable)
            var title = new Label("UI Validation & Analytics");
            title.style.fontSize = 14;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            title.style.marginLeft = 10;
            container.Add(title);

            // Search Field
            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Row;
            searchContainer.style.marginLeft = 10;
            searchContainer.style.marginRight = 10;
            searchContainer.style.marginBottom = 10;
            container.Add(searchContainer);

            var searchLabel = new Label("Filter:");
            searchLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            searchLabel.style.minWidth = 50;
            searchLabel.style.marginRight = 5;
            searchContainer.Add(searchLabel);

            var searchField = new TextField();
            searchField.value = searchText;
            searchField.style.flexGrow = 1;
            searchField.style.marginRight = 5;
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue;
                RefreshViewInfos();
            });
            searchContainer.Add(searchField);

            var sortLabel = new Label("Sort:");
            sortLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            sortLabel.style.minWidth = 35;
            sortLabel.style.marginRight = 5;
            searchContainer.Add(sortLabel);

            var sortDropdown = new DropdownField(new List<string> { "By Group", "By Name" }, sortMode);
            sortDropdown.style.width = 100;
            sortDropdown.RegisterValueChangedCallback(evt =>
            {
                sortMode = sortDropdown.index;
                RefreshViewInfos();
            });
            searchContainer.Add(sortDropdown);

            // Scrollable Content
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            container.Add(scrollView);

            // Statistics Section
            var statsContainer = new VisualElement();
            statsContainer.name = "stats-container";
            statsContainer.style.flexDirection = FlexDirection.Row;
            statsContainer.style.paddingTop = 10;
            statsContainer.style.paddingLeft = 10;
            statsContainer.style.paddingBottom = 10;
            statsContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.2f);
            statsContainer.style.marginBottom = 10;
            statsContainer.style.marginLeft = 10;
            statsContainer.style.borderTopLeftRadius = 5;
            statsContainer.style.borderTopRightRadius = 5;
            statsContainer.style.borderBottomLeftRadius = 5;
            statsContainer.style.borderBottomRightRadius = 5;
            scrollView.Add(statsContainer);

            // UI List Section
            var listTitle = new Label("UI Views");
            listTitle.style.fontSize = 12;
            listTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            listTitle.style.marginLeft = 10;
            listTitle.style.marginBottom = 5;
            scrollView.Add(listTitle);

            var viewInfoContainer = new VisualElement();
            viewInfoContainer.name = "viewInfo-content";
            viewInfoContainer.style.paddingLeft = 10;
            viewInfoContainer.style.paddingRight = 10;
            scrollView.Add(viewInfoContainer);

            return container;
        }

        public void RefreshViewInfos()
        {
            var views = UIValidator.GetViewPaths(settings.prefabPath).Values.ToList();

            var statsContainer = this.Q<VisualElement>("stats-container");
            statsContainer.Clear();

            var infoContent = new VisualElement();
            infoContent.style.width = Length.Percent(40);
            statsContainer.Add(infoContent);

            var statsTitle = new Label("Statistics");
            statsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            statsTitle.style.marginBottom = 5;
            infoContent.Add(statsTitle);

            var statsParent = new VisualElement();
            statsParent.style.flexDirection = FlexDirection.Row;
            statsParent.style.flexWrap = Wrap.Wrap;
            infoContent.Add(statsParent);

            var groupCount = settings.groups?.Count ?? 0;
            var viewCount = views.Count;
            var missingGroupViews = views.Where(v => v.Setting.group < 0 || v.Setting.group >= groupCount).Count();

            AddStatItem(statsParent, "Total Views", viewCount.ToString(), new Color(0.3f, 0.6f, 0.9f));
            AddStatItem(statsParent, "Total Groups", groupCount.ToString(), new Color(0.5f, 0.7f, 0.4f));
            AddStatItem(statsParent, "Missing Used", missingGroupViews.ToString(), new Color(0.9f, 0.6f, 0.3f));


            var result = UIValidator.ValidateUIPath(settings.prefabPath);

            var analyticsContent = new VisualElement();
            statsContainer.Add(analyticsContent);

            var analyticTitle = new Label("Analytics");
            analyticTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            analyticTitle.style.marginBottom = 5;
            analyticsContent.Add(analyticTitle);

            var analyticsParent = new VisualElement();
            analyticsParent.style.flexDirection = FlexDirection.Row;
            analyticsParent.style.flexWrap = Wrap.Wrap;
            analyticsContent.Add(analyticsParent);


            var item = AddStatItem(analyticsParent, "missing paths", result.MissingPrefabUIPaths.Count.ToString(), new Color(0.8f, 0.9f, 0.3f));
            item.tooltip = string.Join("\n", result.MissingPrefabUIPaths);

            item = AddStatItem(analyticsParent, "unmarked prefabs", result.UnmarkedPrefabs.Count.ToString(), new Color(0.8f, 0.9f, 0.3f));
            item.tooltip = string.Join("\n", result.UnmarkedPrefabs);

            item = AddStatItem(analyticsParent, "multiple mapping", result.MultipleMapping.SelectMany(p => p.Value).Count().ToString(), new Color(0.8f, 0.9f, 0.3f));
            item.tooltip = string.Join("\n", result.MultipleMapping.SelectMany(kv => kv.Value.Select(t => $"[UIPath(\"{kv.Key}\")] -> {t.Name}")));

            var viewInfoContainer = this.Q<VisualElement>("viewInfo-content");
            viewInfoContainer.Clear();

            if (views.Count == 0)
            {
                var noViewLabel = new Label("No views found in prefab path");
                noViewLabel.style.marginTop = 10;
                noViewLabel.style.fontSize = 12;
                noViewLabel.style.color = Color.grey;
                viewInfoContainer.Add(noViewLabel);
                return;
            }

            var groups = settings.groups;
            var hasSearchFilter = !string.IsNullOrWhiteSpace(searchText);

            // Apply search filter
            var filteredViews = views;
            if (hasSearchFilter)
            {
                filteredViews = views.Where(v => v.name.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (sortMode == 0) // By Group
            {
                var viewGroups = filteredViews.GroupBy(p => p.Setting.group).OrderBy(p =>
                {
                    if (p.Key >= 0 && p.Key < groups.Count)
                        return groups[p.Key].depth;
                    return 10000;
                });

                foreach (var list in viewGroups)
                {
                    var i = list.Key;
                    var query = list.OrderBy(p => p.Setting.priority).ToList();

                    foreach (var view in query.OrderBy(p => p.Setting.priority))
                    {
                        AddUIItem(viewInfoContainer, view);
                    }
                }
            }
            else // By Name
            {
                var sortedViews = filteredViews.OrderBy(v => v.name).ToList();

                foreach (var view in sortedViews)
                {
                    AddUIItem(viewInfoContainer, view);
                }
            }
        }

        private VisualElement AddUIItem(VisualElement container, BaseView view)
        {
            var path = AssetDatabase.GetAssetPath(view);

            var viewRow = new VisualElement();
            viewRow.name = view.name;
            viewRow.style.flexDirection = FlexDirection.Row;
            viewRow.style.marginBottom = 2;
            viewRow.style.alignItems = Align.Center;
            viewRow.style.paddingTop = 2;
            viewRow.style.paddingBottom = 2;
            viewRow.style.paddingLeft = 5;
            viewRow.style.paddingRight = 5;
            viewRow.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.3f);

            viewRow.style.borderTopLeftRadius = 3;
            viewRow.style.borderTopRightRadius = 3;
            viewRow.style.borderBottomLeftRadius = 3;
            viewRow.style.borderBottomRightRadius = 3;
            container.Add(viewRow);

            var viewButton = new Button(() => Selection.activeObject = view)
            {
                text = view.name,
                tooltip = path,
            };
            viewButton.style.flexGrow = 1;
            viewButton.style.height = 22;
            viewButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            viewButton.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            viewButton.style.marginRight = 5;
            viewRow.Add(viewButton);

            // Group Dropdown
            var groups = settings.groups;
            var groupChoices = new List<string>();
            for (int g = 0; g < groups.Count; g++)
            {
                groupChoices.Add($"{groups[g].name}");
            }

            var isValidGroup = view.Setting.group >= 0 && view.Setting.group < groups.Count;
            if (!isValidGroup)
            {
                groupChoices.Add("Missing");
            }
            var selectedGroupIndex = isValidGroup ? view.Setting.group : groups.Count;

            var groupDropdown = new DropdownField(groupChoices, selectedGroupIndex);
            groupDropdown.style.width = 120;
            groupDropdown.style.marginRight = 5;
            groupDropdown.RegisterValueChangedCallback(evt =>
            {
                var newGroupIndex = groupChoices.IndexOf(evt.newValue);
                if (newGroupIndex >= 0 && newGroupIndex < groups.Count && newGroupIndex != view.Setting.group)
                {
                    var so = new SerializedObject(view);
                    var settingProp = so.FindProperty("Setting");
                    var groupProp = settingProp.FindPropertyRelative("group");
                    groupProp.intValue = newGroupIndex;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(view);
                    AssetDatabase.SaveAssets();
                    RefreshViewInfos();
                }
            });

            viewRow.Add(groupDropdown);

            // Priority Dropdown (using enum sorted by value)
            var priorityValues = System.Enum.GetValues(typeof(Priority)).Cast<Priority>().OrderBy(p => (int)p).ToList();
            var priorityChoices = priorityValues.Select(p => p.ToString()).ToList();
            var currentPriorityName = view.Setting.priority.ToString();

            var priorityDropdown = new DropdownField(priorityChoices, currentPriorityName);
            priorityDropdown.style.width = 100;
            priorityDropdown.RegisterValueChangedCallback(evt =>
            {
                if (System.Enum.TryParse<Priority>(evt.newValue, out var newPriority))
                {
                    if (newPriority != view.Setting.priority)
                    {
                        var so = new SerializedObject(view);
                        var settingProp = so.FindProperty("Setting");
                        var priorityProp = settingProp.FindPropertyRelative("priority");
                        priorityProp.intValue = (int)newPriority;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(view);
                        AssetDatabase.SaveAssets();
                        RefreshViewInfos();
                    }
                }
            });
            viewRow.Add(priorityDropdown);

            return viewRow;
        }

        private VisualElement AddStatItem(VisualElement parent, string label, string value, Color color)
        {
            var statItem = new VisualElement();
            statItem.style.marginRight = 15;
            statItem.style.marginBottom = 5;
            parent.Add(statItem);

            var statLabel = new Label(label);
            statLabel.style.fontSize = 10;
            statLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            statItem.Add(statLabel);

            var statValue = new Label(value);
            statValue.style.fontSize = 16;
            statValue.style.unityFontStyleAndWeight = FontStyle.Bold;
            statValue.style.color = color;
            statItem.Add(statValue);

            return statItem;
        }
    }
}