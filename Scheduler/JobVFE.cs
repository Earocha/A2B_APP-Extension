using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using A2B_APP_Extension.Controllers.Sharefile;

namespace A2B_APP_Extension.Schedulers
{
	public class JobVFE : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			VideoFrameExtractor StartVideoExtractor = new VideoFrameExtractor();
			await StartVideoExtractor.Start();
		}
	}
}

