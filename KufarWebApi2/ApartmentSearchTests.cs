using Newtonsoft.Json.Linq;
using Xunit;

namespace KufarWebApi2
{
    public class ApartmentSearchTests
    {
        [Fact]
        public void TestPointInField()
        {
            var field = new List<Point>
        {
            new Point(0, 0),
            new Point(5, 0),
            new Point(5, 5),
            new Point(0, 5)
        };

            var insidePoint = new Point(3, 3);
            var outsidePoint = new Point(6, 6);

            Assert.True(Program.IsPointInField(insidePoint, field));
            Assert.False(Program.IsPointInField(outsidePoint, field));
        }

        [Fact]
        public void TestFindApartmentsInField()
        {
            var apartments = new List<JObject>
        {
            new JObject
            {
                ["ad_parameters"] = new JArray
                {
                    new JObject
                    {
                        ["p"] = "coordinates",
                        ["v"] = new JArray { 27.536975, 53.868732 }
                    }
                }
            },
            new JObject
            {
                ["ad_parameters"] = new JArray
                {
                    new JObject
                    {
                        ["p"] = "coordinates",
                        ["v"] = new JArray { 27.600, 53.900 }
                    }
                }
            }
        };

            var field = new List<Point>
        {
            new Point(27.536, 53.868),
            new Point(27.574, 53.898),
            new Point(27.574, 53.914),
            new Point(27.546, 53.914)
        };

            var result = Program.FindApartmentsInField(apartments, field);

            Assert.Single(result);
            Assert.Equal(27.536975, result[0]["ad_parameters"][0]["v"][0].Value<double>());
        }

        [Fact]
        public void TestInvalidPolygon()
        {
            var apartments = new List<JObject>();
            var invalidField = new List<Point>
        {
            new Point(27.546, 53.898)
        };

            Assert.Throws<ArgumentException>(() => Program.FindApartmentsInField(apartments, invalidField));
        }
    }

}
