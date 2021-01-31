﻿using UnityEngine;

namespace QSB.Utility
{
	public delegate void EnableDisableEvent();

	public class OnEnableDisableTracker : MonoBehaviour
	{
		public event EnableDisableEvent OnEnableEvent;
		public event EnableDisableEvent OnDisableEvent;

		public MonoBehaviour AttachedComponent;

		private ComponentState _wasEnabled = ComponentState.NotChecked;

		private void Update()
		{
			if (AttachedComponent == null)
			{
				return;
			}
			var state = AttachedComponent.isActiveAndEnabled ? ComponentState.Enabled : ComponentState.Disabled;
			if (_wasEnabled != state)
			{
				_wasEnabled = state;
				if (state == ComponentState.Enabled)
				{
					OnEnableEvent?.Invoke();
				}
				else
				{
					OnDisableEvent?.Invoke();
				}
			}
		}
	}

	internal enum ComponentState
	{
		NotChecked = 0,
		Enabled = 1,
		Disabled = 2
	}
}