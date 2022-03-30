using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A2B_APP_Extension.Model;
using System.Threading;
using OpenQA.Selenium.Support.UI;

namespace A2B_APP_Extension.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlerController : ControllerBase
    {

        [HttpPost("execute")]
        public async Task<IActionResult> TestAPICrawler([FromBody] WebCrawler crawlerItem)
        {
            WebCrawler CrawlerObj = new WebCrawler();
            string Link = crawlerItem.WebLink;
            string ClickLink = crawlerItem.ClickLink;
            string TitleXPath = crawlerItem.Title;
            string ShortDescXPath = crawlerItem.ShortDescription;
            string BodyXPath = crawlerItem.Body;
            IWebDriver driver = new ChromeDriver(@"C:/Users/120_Remote_DT/Desktop/A2B_APP-Extension/A2B_APP_Extension/bin/chromedriver_v99");
            driver.Navigate().GoToUrl(Link);

            try
            {
                Thread.Sleep(5000);
                driver.FindElement(By.XPath(ClickLink)).Click();

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                driver.SwitchTo().Window(driver.WindowHandles.Last());

                wait.Until(driver => driver.FindElement(By.XPath(TitleXPath)));
                CrawlerObj.Client = crawlerItem.Client;
                CrawlerObj.Title = driver.FindElement(By.XPath(TitleXPath)).Text;
                CrawlerObj.Body = driver.FindElement(By.XPath(BodyXPath)).Text;
                CrawlerObj.ShortDescription = driver.FindElement(By.XPath(ShortDescXPath)).Text;
                CrawlerObj.WebCrawlerScheduleTime = crawlerItem.WebCrawlerScheduleTime;
                CrawlerObj.Screenshot = crawlerItem.Screenshot;

                if (CrawlerObj.Screenshot != "false")
                {
                    Screenshot Image = ((ITakesScreenshot)driver).GetScreenshot();
                    CrawlerObj.Screenshot = Image.AsBase64EncodedString;
                }

                Thread.Sleep(2000);
                Console.WriteLine("Image bse64 ===>", CrawlerObj.Screenshot);
                Console.WriteLine("Client==>" + CrawlerObj.Client);
                Console.WriteLine("Title ==>" + CrawlerObj.Title);
                Console.WriteLine("ShortDescription ==>" + CrawlerObj.ShortDescription);
                Console.WriteLine("Body ==>" + CrawlerObj.Body);

                driver.Close();
                driver.Quit();
                
                return Ok(CrawlerObj);
            }
            catch (Exception err)
            {

                Console.WriteLine(err.ToString());
                driver.Close();
                driver.Quit();
                return BadRequest(CrawlerObj);
            }
        }
    }
}
