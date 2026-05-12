using System;
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
    }
}
