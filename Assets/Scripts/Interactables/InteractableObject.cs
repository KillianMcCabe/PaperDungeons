﻿using UnityEngine;

namespace PaperDungeons
{
    public abstract class InteractableObject : MonoBehaviour, IInteractable
    {
        public abstract void Interact();

        // TODO: right-click = inspect
        // public abstract void Inspect();
    }
}
