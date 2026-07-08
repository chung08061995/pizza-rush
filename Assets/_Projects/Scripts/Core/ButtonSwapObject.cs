using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DraftUtils
{
    public class ButtonSwapObject : Button
    {
        [Header("Custom Button")]
        [SerializeField] private Transform objNormal;
        [SerializeField] private Transform objHighlighted;
        [SerializeField] private Transform objPressed;
        [SerializeField] private Transform objSelected;
        [SerializeField] private Transform objDisabled;

        protected override void Start()
        {
            base.Start();
        }
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            switch (state)
            {
                case SelectionState.Normal:
                    SelectOneState(objNormal);
                    break;
                case SelectionState.Highlighted:
                    SelectOneState(objHighlighted);
                    break;
                case SelectionState.Pressed:
                    SelectOneState(objPressed);
                    break;
                case SelectionState.Selected:
                    SelectOneState(objSelected);
                    break;
                case SelectionState.Disabled:
                    SelectOneState(objDisabled);
                    break;
            }
        }

        private void SelectOneState(Transform obj)
        {
            objNormal.gameObject.SetActive(obj == objNormal);
            objHighlighted.gameObject.SetActive(obj == objHighlighted);
            objPressed.gameObject.SetActive(obj == objPressed);
            objSelected.gameObject.SetActive(obj == objSelected);
            objDisabled.gameObject.SetActive(obj == objDisabled);
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ButtonSwapObject))]
    public class ButtonSwapObjectEditor : Editor
    {
        ButtonSwapObject mtarget;
        private void OnEnable()
        {
            mtarget = target as ButtonSwapObject;
        }
    }
#endif
}
