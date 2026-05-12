using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class ExternalFlightsTests
{
    private IWebDriver driver;
    private WebDriverWait wait;
    private const double MaxAcceptablePrice = 1000; // Előre beállított árlimit

    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    [TearDown]
    public void Teardown()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch { }
    }

    [Test]
    public void BlazeDemo_MexicoCityToDublin_ShouldHaveAtLeastThreeFlights()
    {
        // Arrange
        driver.Navigate().GoToUrl("https://blazedemo.com");

        // Wait for selects to be present
        var fromSelect = wait.Until(ExpectedConditions.ElementExists(By.Name("fromPort")));
        var toSelect = wait.Until(ExpectedConditions.ElementExists(By.Name("toPort")));

        // Select departure and destination
        var selectFrom = new SelectElement(fromSelect);
        selectFrom.SelectByText("Mexico City");

        var selectTo = new SelectElement(toSelect);
        selectTo.SelectByText("Dublin");

        // Submit search
        var findButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("input[type='submit']")));
        findButton.Click();

        // Assert - wait for results table and ensure at least 3 rows
        wait.Until(ExpectedConditions.ElementExists(By.CssSelector("table.table")));
        var rows = driver.FindElements(By.CssSelector("table.table tbody tr"));
        rows.Count.Should().BeGreaterThanOrEqualTo(3, because: "there should be at least three flights between Mexico City and Dublin on BlazeDemo");

        // Check prices and capture screenshot if cheap flight found
        CheckFlightPricesAndCaptureIfCheap(rows);
    }

    private void CheckFlightPricesAndCaptureIfCheap(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> rows)
    {
        bool cheapFlightFound = false;

        foreach (var row in rows)
        {
            try
            {
                // Extract price from the last column (Price)
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count > 0)
                {
                    var priceText = cells[cells.Count - 1].Text; // Last column is typically Price
                    if (double.TryParse(priceText.Replace("$", "").Trim(), out double price))
                    {
                        if (price < MaxAcceptablePrice)
                        {
                            cheapFlightFound = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
                // Continue to next row if parsing fails
            }
        }

        // If cheap flight found, take a screenshot
        if (cheapFlightFound)
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        try
        {
            var screenshotDriver = driver as ITakesScreenshot;
            if (screenshotDriver != null)
            {
                // Desktop path
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filename = Path.Combine(desktopPath, $"CheapFlight_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                var screenshot = screenshotDriver.GetScreenshot();
                // SaveAsFile expects the base64 screenshot to be written as PNG bytes
                File.WriteAllBytes(filename, screenshot.AsByteArray);

                TestContext.Out.WriteLine($"Screenshot saved: {filename}");
            }
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Failed to save screenshot: {ex.Message}");
        }
    }
}
