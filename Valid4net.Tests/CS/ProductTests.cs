using System;

using Valid4net.Tests.CS.Models;

using Xunit;

namespace Valid4net.Tests.CS
{
    public class ProductTests
    {
        [Fact]
        public void HasErrorsTrueTest()
        {
            // Object model
            var product = new Product();

            // Rules definition
            product.AddRule(nameof(product.Title), "Title length must be between 5 and 10!!",
                p => (p.Title?.Length > 5) && (p.Title?.Length <= 20));
            // More rules
            product.Title = "Title";

            // Assert
            Assert.True(product.HasErrors);
        }

        [Fact]
        public void HasErrorsFalseTest()
        {
            // Object model
            var product = new Product();

            // Rules definition
            product.AddRule(nameof(product.Title), "Title length must be between 5 and 10!!",
                p => (p.Title?.Length > 5) && (p.Title?.Length <= 20));
            // More rules

            // Properties values assignment
            product.Id = Guid.NewGuid().ToString();
            product.Title = "Product's title";

            // Assert
            Assert.False(product.HasErrors);
        }
    }
}
