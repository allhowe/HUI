using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    [Serializable]
    public struct UIGroupInfo
    {
        public string name;
        public int depth;
    }

    public class UIGroup : IEnumerable<BaseUI>
    {
        public Canvas Canvas { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public UIGroupInfo Info { get; private set; }

        public Transform transform => Canvas.transform;

        public int Count => uis.Count;

        private int counter;


        internal SortedList<int, BaseUI> uis;
        private Dictionary<BaseUI, int> map;

        public UIGroup(UIGroupInfo info, Canvas templete) {
            this.Info = info;
            uis = new SortedList<int, BaseUI>(Comparer<int>.Create((a, b) => a.CompareTo(b)));
            map = new Dictionary<BaseUI, int>();

            Canvas = GameObject.Instantiate(templete, templete.transform.parent);
            Canvas.gameObject.SetActive(true);
            Canvas.name = info.name;
            Canvas.sortingOrder = info.depth;

            CanvasGroup = Canvas.GetComponent<CanvasGroup>();
        }
        private void CalculateSiblingOptimized(BaseUI ui, Priority priority) {
            if (map.Remove(ui, out int key)) {
                uis.Remove(key);
            }

            key = (int)priority * 10000 + counter++;
            uis[key] = ui;
            map[ui] = key;

            for (int i = 0; i < uis.Count; i++)
            {
                uis.Values[i].View.transform.SetSiblingIndex(i);
            }
        }

        internal void AddUI(BaseUI ui,Priority priority) {
            ui.View.transform.SetParent(transform, false);
            CalculateSiblingOptimized(ui, priority);
        }

        internal void RemoveUI(BaseUI ui) {
            if(map.Remove(ui,out int key)) {
                uis.Remove(key);
            }
        }

        public void Refresh()
        {
            foreach (var item in this)
            {
                item.Refresh();
            }
        }

        public IEnumerator<BaseUI> GetEnumerator() {
            return uis.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }


    public class UIGroupCollection : IEnumerable<UIGroup>
    {
        private List<UIGroup> groups;
        public UIGroupCollection(UISettings settings,Canvas templete)
        {
            var infos = settings.groups;
            groups = new List<UIGroup>(infos.Count);

            for (int i = 0; i < infos.Count; i++)
            {
                var info = infos[i];
                var group = new UIGroup(info, templete);
                groups.Add(group);

                if (Max == null || info.depth > Max.Info.depth) Max = group;
                if (Min == null || info.depth < Min.Info.depth) Min = group;
            }
        }

        public UIGroup this[int index] => groups[index];
        public UIGroup Max { get; private set; }
        public UIGroup Min { get; private set; }
        public int Count => groups.Count;

        public void AddToGroup(BaseUI ui)
        {
            var setting = ui.View.Setting;
            var group = this[setting.group];
            ui.Group = group;
            group.AddUI(ui, setting.priority);
        }
        public void RemoveFromGroup(BaseUI ui) {
            ui.Group.RemoveUI(ui);
        }

        public IEnumerator<UIGroup> GetEnumerator()
        {
            return groups.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}