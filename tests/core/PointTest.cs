public class PointTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Point_IsInitialized()
    {
        Point p = new Point(0, 1);
        Assert.Pass();
    }
}