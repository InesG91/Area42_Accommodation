using Xunit;
using Moq;
using Core.Domain.Models;
using Core.Domain.Services;
using Core.Domain.Interfaces;
using System;

namespace Area42.Tests.Unit
{
    public class GuestRegistrationTests
    {
        private readonly Mock<IGuestRepository> _mockGuestRepository;
        private readonly GuestService _guestService;

        public GuestRegistrationTests()
        {
            _mockGuestRepository = new Mock<IGuestRepository>();
            _guestService = new GuestService(_mockGuestRepository.Object);
        }

        [Fact]
        public void StoreGuest_ValidAdultGuest_ReturnsGeneratedGuestId()
        {
            // Arrange
            var expectedGuestId = 123;
            _mockGuestRepository.Setup(r => r.Save(It.IsAny<Guest>())).Returns(expectedGuestId);

            var validGuest = new Guest(
                guestID: 0,
                firstName: "John",
                lastName: "Doe",
                dateOfBirth: new DateTime(1990, 5, 15), // Adult guest
                email: "john.doe@email.com"
            );

            // Act
            var result = _guestService.StoreGuest(validGuest);

            // Assert
            Assert.Equal(expectedGuestId, result);
            _mockGuestRepository.Verify(r => r.Save(validGuest), Times.Once);
        }

        [Fact]
        public void CreateGuest_Under18_ThrowsInvalidOperationException()
        {
            // Arrange
            var under18DateOfBirth = DateTime.Today.AddYears(-17); // 17 years old

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                new Guest(0, "Minor", "Person", under18DateOfBirth, "minor@email.com"));

            Assert.Equal("Guest must be at least 18 years old to book.", exception.Message);
        }
    }
}
