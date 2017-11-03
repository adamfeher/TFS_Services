using RainforestExcavator.UI.ViewModels;
using System;
using System.Collections.Generic;

namespace RainforestExcavator.Core
{
    public class TaskManager
    {
        // TaskManager instance keeps tracking of running tasks based on a registration/deregistration mechanism.
        // Tasks are tracked based on whether multiple instances can run without introducing conflicts.
        // In that sense, they are tracked/registered as Specific or Generic tasks.

        // Specific task instances are tracked in a dictionary at a key given at time of registration.
        // This helps to disallow multiple initializations of a task as well as store the task at a location
        // where it can be accessed for state checks.
        // Example -> Aggregator task -> multiple instances would cause file conflicts

        // Generic task instance are tracked in a simple counter, their instances can run overlapped
        // without causing detrimental harm.
        // Example -> fetch Projects/Plans/Suites

        private int NumGeneralSpinningTasks;                // counts number of tasks that can be generalized
        private Dictionary<string, object> SpecificTasks;   // stores special tasks in a dict so that only one may exist
        private MainWindowViewModel MainWindowViewModel;
        
        /// <summary>
        /// Check if any tasks are currently being managed.
        /// </summary>
        public bool HasRunningTasks
        {
            get
            {
                return (NumGeneralSpinningTasks + SpecificTasks.Count) > 0;
            }
        }

        public TaskManager(MainWindowViewModel mwvm)
        {
            this.NumGeneralSpinningTasks = 0;
            this.SpecificTasks = new Dictionary<string, object>();
            this.MainWindowViewModel = mwvm;
        }
        /// <summary>
        /// Calls to this will increment the generic task counter.
        /// </summary>
        /// This is intended to count tasks for which the tool can have multiple instances running at once.
        public void AddTask()
        {
            NumGeneralSpinningTasks++;
            UpdateSpinner();
        }
        /// <summary>
        /// Calls to this will add a task to the dict where the key is the name provided, but only if it does not already exist.
        /// </summary>
        /// <returns>True if added to the dict, false if another instance is already running.</returns>
        public bool AddSpecificTask(string taskName, object task)
        {
            if (SpecificTasks.ContainsKey(taskName)) { return false; }
            SpecificTasks.Add(taskName, task);
            UpdateSpinner();
            return true;
        }
        /// <summary>
        /// Decrements the counter for generic tasks currently running.
        /// </summary>
        public void RemoveTask()
        {
            NumGeneralSpinningTasks--;
            UpdateSpinner();
        }
        /// <summary>
        /// Removes the specific task from the dict with the provided name as key.
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public bool RemoveSpecificTask(string taskName)
        {
            bool result = SpecificTasks.Remove(taskName);
            UpdateSpinner();
            return result;
        }
        /// <summary>
        /// Returns whether or not a dict entry exists with the given key.
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public bool ContainsSpecificTask(string taskName)
        {
            return SpecificTasks.ContainsKey(taskName);
        }
        /// <summary>
        /// Triggers the spinner on MainWindow based on currently running tasks.
        /// </summary>
        private void UpdateSpinner()
        {
            this.MainWindowViewModel.SpinnerOn = this.HasRunningTasks;
        }
    }
}
