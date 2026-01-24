using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ddchttp.Services;

namespace ddchttp.Controllers;

[TestClass]
public class InputControllerTests
{
    [TestMethod]
    public void extract_validtypes_success()
    {
        var options = new Mock<IOptions<InputControllerSettings>>();

        options.Setup(x => x.Value).Returns(new InputControllerSettings {
            AvailableInputs = new[] {
                new AvailableInput { Swap = new[] { "a", "c" } },
                new AvailableInput { Swap = new[] { "b", "c" } },
            }
        });

        var controller = new InputController(null!, options.Object);

        var result = controller.ExtractValidSwapTypes();

        CollectionAssert.AreEqual(new[] { "a", "c", "b" }, result);
    }

    [TestMethod]
    [DataRow("a", "0.0.0.1", "name2")]
    [DataRow("a", "0.0.0.2", "name1")]
    [DataRow("b", "0.0.0.2", "name3")]
    [DataRow("b", "0.0.0.3", "name2")]
    public void querycode_byswapandip_success(string swap, string ip, string expected)
    {
        var options = new Mock<IOptions<InputControllerSettings>>();

        options.Setup(x => x.Value).Returns(new InputControllerSettings {
            AvailableInputs = new[] {
                new AvailableInput { Swap = new[] { "a"}, Ip = "0.0.0.1", Name = "name1" },
                new AvailableInput { Swap = new[] { "a", "b" }, Ip = "0.0.0.2", Name = "name2" },
                new AvailableInput { Swap = new[] { "b" }, Ip = "*", Name = "name3" },
            }
        });

        var controller = new InputController(null!, options.Object);

        var result = controller.QueryNameBySwapAndIp(swap, ip);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void querycode_byswapandip_null_success()
    {
        var displayProvider = new Mock<IDisplayProvider>();
        var options = new Mock<IOptions<InputControllerSettings>>();

        options.Setup(x => x.Value).Returns(new InputControllerSettings());

        var controller = new InputController(displayProvider.Object, options.Object);

        var result = controller.QueryNameBySwapAndIp("a", "b");

        Assert.IsNull(result);
    }    
}
