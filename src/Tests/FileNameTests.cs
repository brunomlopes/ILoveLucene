using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Plugins.Tasks;
using Xunit;

namespace Tests
{
    public class FileNameTests
    {
        private Regex regexp;

        [Fact]
        public void RegexpReplaceWorks()
        {
            Assert.Equal("_text_ here", regexp.Replace("<text> here", "_"));
            Assert.Equal("_text_ here", regexp.Replace(":text> here", "_"));
            Assert.Equal("_text_ here", regexp.Replace("\"text\\ here", "_"));
            Assert.Equal("_text_ here", regexp.Replace("?text* here", "_"));
            Assert.Equal("_text_ here", regexp.Replace("|text> here", "_"));
        }

        public FileNameTests()
        {
            regexp = new System.Text.RegularExpressions.Regex(@"[<>:""/\\|?*]");
        }
    }
    
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
            var taskRepository = new TaskRepository();

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
            var taskRepository = new TaskRepository();

            var task = new Task("do stuff");
            task.Start();
            
            taskRepository.CreateTask(task);

            Thread.Sleep(TimeSpan.FromSeconds(1)); // TODO: remove this sleep
            var loadedTask = taskRepository.FromFileName(task.FileName);
            loadedTask.Stop();

            Assert.Equal(1, loadedTask.Durations.Count);
        }
      
    }
}