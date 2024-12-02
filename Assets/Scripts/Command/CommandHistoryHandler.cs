using System.Collections;
using System.Collections.Generic;
using Command;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Command
{
    public class CommandHistoryHandler : MonoBehaviour
    {
        private FixedSizeStack<ICommand> undoStack;
        private FixedSizeStack<ICommand> redoStack;

        [SerializeField] 
        private AudioClip placeSFX;

        public UnityEvent<ICommand> onExecuteCommand;
        public UnityEvent<ICommand> onUndoCommand;
        void Start()
        {
            undoStack = new FixedSizeStack<ICommand>(200);
            redoStack = new FixedSizeStack<ICommand>(200);
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            SoundManager.Instance.PlaySFX(placeSFX);
            onExecuteCommand.Invoke(command);
            undoStack.Push(command);
            redoStack.Clear();
        }

        public void ClearStack()
        {
            undoStack?.Clear();
            redoStack?.Clear();
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
            onUndoCommand.Invoke(lastCommand);
            
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
            onExecuteCommand.Invoke(lastCommand);
        
            undoStack.Push(lastCommand);
        }
    }
}
