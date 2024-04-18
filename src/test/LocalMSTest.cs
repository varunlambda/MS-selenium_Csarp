using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
/* For using Remote Selenium WebDriver */
using OpenQA.Selenium.Remote;
using System;
using System.Threading;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;

[assembly: Parallelize(Workers = 5, Scope = ExecutionScope.MethodLevel)]

namespace ParallelLTSelenium
{
    [TestClass]
    public class ParallelLTTests
    {
        IWebDriver driver;

        /* Profile - https://accounts.lambdatest.com/detail/profile */
        String username = "Username";
        String accesskey = "AccessKey";
        String gridURL = "@hub.lambdatest.com/wd/hub";



        DesiredCapabilities capabilities;

        [TestInitialize]
        public async Task setupInitAsync()
        {
            capabilities = new DesiredCapabilities();

            capabilities.SetCapability("user", username);
            capabilities.SetCapability("accessKey", accesskey);


            // Define the API endpoint
            string url = "https://api.lambdatest.com/automation/api/v1/user-files";

            // Define the file path
            string filePath = "D:\\Varun\\MSTest-Selenium-Sample\\image_sample.png"; // Update with the correct file path

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "<Basic_Auth>"); // Update Basic Auth here

                using (MultipartFormDataContent formData = new MultipartFormDataContent())
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamContent fileContent = new StreamContent(fileStream))
                        {
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                            formData.Add(fileContent, "files", Path.GetFileName(filePath));
                            request.Content = formData;

                            HttpResponseMessage response = await client.SendAsync(request);
                            response.EnsureSuccessStatusCode();
                            string responseContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(responseContent);
                        }
                    }
                }
            }
        }

        [DataTestMethod]
        [DataRow("chrome", "121.0", "Windows 10")]
        // [DataRow("MicrosoftEdge", "18.0", "Windows 10")]
        // [DataRow("Firefox", "70.0", "macOS High Sierra")]
        // [DataRow("Safari", "12.0", "macOS Mojave")]

        [TestMethod]
        public void LT_ToDo_Test(String browser, String version, String os)
        {
            String itemName = "Yey, Let's add it to list";

            Dictionary<string, string> initCaps = new Dictionary<string, string>();
            string[] ltFile = new string[] { "image_sample.png" };


            capabilities.SetCapability("browserName", browser);
            capabilities.SetCapability("version", version);
            capabilities.SetCapability("platform", os);
            capabilities.SetCapability("build", "LT ToDoApp using MsTest in Parallel on LambdaTest");
            capabilities.SetCapability("name", "LT ToDoApp using MsTest in Parallel on LambdaTest");
            capabilities.SetCapability("lambda:userFiles", ltFile);


            driver = new RemoteWebDriver(new Uri("https://" + username + ":" + accesskey + gridURL), capabilities, TimeSpan.FromSeconds(2000));


            // First verification by using the same file to uplaod in te ongoing session

            driver.Url = "https://the-internet.herokuapp.com/upload";
            Assert.AreEqual("The Internet", driver.Title);

            string filePath1 = "C:\\Users\\ltuser\\Downloads\\image_sample.png";
            IWebElement ChooseFile = driver.FindElement(By.XPath("/html/body/div[2]/div/div[1]/form/input[1]"));
            ChooseFile.SendKeys(filePath1);
            IWebElement upload = driver.FindElement(By.XPath("/html/body/div[2]/div/div[1]/form/input[2]"));
            upload.Click();

            // Second level of verification  by Checking if file is available in download directory of LambdaTest VM

            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
            string fileName = "image_sample.png"; // Update with the actual file name and format
            jsExecutor.ExecuteScript($"lambda-file-exists={fileName}"); // Gives boolean reponse

            System.Threading.Thread.Sleep(10000);
            Console.WriteLine("LT_ToDo_Test Passed");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (driver != null)
                driver.Quit();
        }
    }
}
