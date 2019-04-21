using System;
using System.Threading;
using Core.API;
using Plugins.Tasks;
using Xunit;

namespace Tests
{
    public class TasksTests
    {

        [Fact]
        public void EnsureIDidNotMessUpTasks()
        {
            var task = new Task("do stuff");
            task.Start();
            task.Stop();

            task.Start();
            task.Stop();

            var description = task.Description;

            // just make sure it doesn't blow up

        } 
        
        [Fact]
        public void RepositoryWorks()
        {
            var taskRepository = new TaskRepository() { Configuration = new CoreConfiguration(".", ".") };

            var task = new Task("do stuff");
            task.Start();
            task.Stop();

            task.Start();
            task.Stop();

            taskRepository.CreateTask(task);

            var loadedTask = taskRepository.FromFileName(task.FileName);
            Assert.Equal(task.Durations.Count, loadedTask.Durations.Count);
            Assert.Equal(task.Name, loadedTask.Name);


        }
        
        [Fact]
        public void CanStartSaveGetAndStop()
        {
            var taskRepository = new TaskRepository() { Configuration = new CoreConfiguration(".",".") };
            

            var task = new Task("do stuff");
            task.Start();
            
            taskRepository.CreateTask(task);

            Thread.Sleep(TimeSpan.FromSeconds(1)); // TODO: remove this sleep
            var loadedTask = taskRepository.FromFileName(task.FileName);
            loadedTask.Stop();

            Assert.Single(loadedTask.Durations);
        }
      
    }
}