using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Command
{
    public interface ICommand
    {
        public void Execute();
        public void Undo();
    }
}
