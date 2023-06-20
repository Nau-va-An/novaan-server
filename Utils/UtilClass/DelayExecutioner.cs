using System;
namespace Utils.UtilClass
{
	public class DelayExecutioner
	{
		private List<Func<Task>> delayedTasks = new List<Func<Task>>();

		public void AppendAction(Func<Task> delayedTask)
		{
            delayedTasks.Add(delayedTask);
		}

		public void Cleanup()
		{
			delayedTasks.Clear();
		}

		public async Task Execute()
		{
			foreach(var delayedTask in delayedTasks)
			{
				await delayedTask();
			}
		}
	}
}

