using System.Collections;
using System.Collections.Generic;
using Command;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Command
{
    public class CommandHistoryHandler : MonoBehaviour
    {
        private Stack<ICommand> undoStack;
        private Stack<ICommand> redoStack;
    
        void Start()
        {
            undoStack = new Stack<ICommand>();
            redoStack = new Stack<ICommand>();
        }

        public void AddCommand(ICommand command)
        {
            undoStack.Push(command);
            redoStack.Clear();
        }

        [Button]
        public void Undo()
        {
            if (undoStack.Count == 0)
            {
                return;
            }
        
            ICommand lastCommand = undoStack.Pop();
            lastCommand.Undo();
        
            redoStack.Push(lastCommand);
        }

        [Button]
        public void Redo()
        {
            if (redoStack.Count == 0)
            {
                return;
            }
        
            ICommand lastCommand = redoStack.Pop();
            lastCommand.Execute();
        
            undoStack.Push(lastCommand);
        }
    }
}
