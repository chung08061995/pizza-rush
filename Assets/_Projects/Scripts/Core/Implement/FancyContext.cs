using FancyScrollView;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DraftUtils
{
    public class FancyContext : FancyScrollRectContext
    {
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
    }
}