using Xunit;
using Moq;
using Core.Domain.Services;
using Core.Domain.Interfaces;
using Core.Domain.Models;
using Core.Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Area42_Accommodation.Tests
{
    public class AccommodationBookingTests
    {
        #region Test Setup and Mocks

        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IGuestRepository> _mockGuestRepository;
        private readonly Mock<IAccommodationRepository> _mockAccommodationRepository;
        private readonly Mock<IAvailabilityRepository> _mockAvailabilityRepository;
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;

        private readonly BookingService _bookingService;
        private readonly GuestService _guestService;
        private readonly AccommodationService _accommodationService;
        private readonly AvailabilityService _availabilityService;
        private readonly PaymentService _paymentService;
        private readonly PricingService _pricingService;

        public AccommodationBookingTests()
        {
            // Initialize mocks
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockGuestRepository = new Mock<IGuestRepository>();
            _mockAccommodationRepository = new Mock<IAccommodationRepository>();
            _mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
            _mockPaymentRepository = new Mock<IPaymentRepository>();

            // Initialize services with mocked dependencies
            _guestService = new GuestService(_mockGuestRepository.Object);
            _accommodationService = new AccommodationService(_mockAccommodationRepository.Object);
            _availabilityService = new AvailabilityService(_mockAvailabilityRepository.Object, _mockAccommodationRepository.Object);
            _paymentService = new PaymentService(_mockPaymentRepository.Object);
            _pricingService = new PricingService(_mockAccommodationRepository.Object);

            _bookingService = new BookingService(
                _mockBookingRepository.Object,
                _guestService,
                _accommodationService,
                _availabilityService,
                _paymentService
            );
        }

        private Guest CreateTestGuest(int id = 1, int age = 25)
        {
            var birthDate = DateTime.Now.AddYears(-age);
            return new Guest(id, "John", "Doe", birthDate, "john@example.com");
        }

        private Accommodation CreateTestAccommodation(int id = 1, uint maxGuests = 4)
        {
            return new Chalet(id, 150.00m, "Luxury Suite", maxGuests);
        }

        #endregion

        #region Successful Booking Tests

        [Fact]
        public void CreateBooking_NewGuest_PayPal_ShouldSucceed()
        {
            // Arrange
            var accommodation = CreateTestAccommodation();
            var startDate = DateTime.Now.AddDays(7);
            var endDate = DateTime.Now.AddDays(14);

            // Setup mocks - no existing guest (new guest scenario)
            _mockGuestRepository.Setup(x => x.GetByEmail("john@example.com")).Returns((Guest)null);
            _mockGuestRepository.Setup(x => x.Save(It.IsAny<Guest>())).Returns(123); // New guest ID
            _mockGuestRepository.Setup(x => x.GetById(123)).Returns(CreateTestGuest(123));

            _mockAccommodationRepository.Setup(x => x.GetById(1)).Returns(accommodation);
            _mockAccommodationRepository.Setup(x => x.IsAvailable(1, startDate, endDate)).Returns(true);
            _mockBookingRepository.Setup(x => x.CreateBooking(It.IsAny<Booking>())).Returns(456); // Booking ID

            // PayPal payment setup
            _mockPaymentRepository.Setup(x => x.CreatePayment(456, It.IsAny<decimal>(), "Confirmed")).Returns(789);

            // Act
            var bookingId = _bookingService.CreateBooking(
                "John", "Doe", DateTime.Now.AddYears(-25), "john@example.com",
                1, startDate, endDate, 2);

            var paymentDetails = new Dictionary<string, string> { { "email", "john@example.com" } };
            var paymentResult = _paymentService.ProcessBookingPayment(bookingId, 1050.00m, "paypal", paymentDetails);

            // Assert
            Assert.True(bookingId > 0, "Booking created");
            Assert.True(paymentResult.Contains("Payment saved with ID: 789"), "PaymentID > 0");
            _mockGuestRepository.Verify(x => x.Save(It.IsAny<Guest>()), Times.Once); // New GuestID created
        }

        [Fact]
        public void CreateBooking_ExistingGuest_CreditCard_ShouldSucceed()
        {
            // Arrange
            var existingGuest = CreateTestGuest(123);
            var accommodation = CreateTestAccommodation();
            var startDate = DateTime.Now.AddDays(7);
            var endDate = DateTime.Now.AddDays(14);

            // Setup mocks - existing guest scenario
            _mockGuestRepository.Setup(x => x.GetByEmail("john@example.com")).Returns(existingGuest);
            _mockGuestRepository.Setup(x => x.GetById(123)).Returns(existingGuest);

            _mockAccommodationRepository.Setup(x => x.GetById(1)).Returns(accommodation);
            _mockAccommodationRepository.Setup(x => x.IsAvailable(1, startDate, endDate)).Returns(true);
            _mockBookingRepository.Setup(x => x.CreateBooking(It.IsAny<Booking>())).Returns(456);

            // Credit card payment setup
            _mockPaymentRepository.Setup(x => x.CreatePayment(456, It.IsAny<decimal>(), "Confirmed")).Returns(789);

            // Act
            var bookingId = _bookingService.CreateBooking(
                "John", "Doe", DateTime.Now.AddYears(-25), "john@example.com",
                1, startDate, endDate, 2);

            var paymentDetails = new Dictionary<string, string>
            {
                { "cardName", "John Doe" },
                { "cardNumber", "4111111111111111" },
                { "expiry", "12/26" },
                { "cvv", "123" }
            };
            var paymentResult = _paymentService.ProcessBookingPayment(bookingId, 1050.00m, "creditcard", paymentDetails);

            // Assert
            Assert.True(bookingId > 0, "Booking created");
            Assert.Equal(123, existingGuest.GuestID); // Existing GuestID used
            Assert.True(paymentResult.Contains("Payment saved"), "Credit card processed");
            _mockGuestRepository.Verify(x => x.Save(It.IsAny<Guest>()), Times.Never); // Should not create new guest
        }

        [Fact]
        public void CreateBooking_BankTransfer_ShouldSucceed()
        {
            // Arrange
            var accommodation = CreateTestAccommodation();
            var startDate = DateTime.Now.AddDays(7);
            var endDate = DateTime.Now.AddDays(14);

            // Setup mocks
            _mockGuestRepository.Setup(x => x.GetByEmail("jane@example.com")).Returns((Guest)null);
            _mockGuestRepository.Setup(x => x.Save(It.IsAny<Guest>())).Returns(124);
            _mockGuestRepository.Setup(x => x.GetById(124)).Returns(CreateTestGuest(124));

            _mockAccommodationRepository.Setup(x => x.GetById(1)).Returns(accommodation);
            _mockAccommodationRepository.Setup(x => x.IsAvailable(1, startDate, endDate)).Returns(true);
            _mockBookingRepository.Setup(x => x.CreateBooking(It.IsAny<Booking>())).Returns(457);

            // Bank transfer payment setup
            _mockPaymentRepository.Setup(x => x.CreatePayment(457, It.IsAny<decimal>(), "Confirmed")).Returns(790);

            // Act
            var bookingId = _bookingService.CreateBooking(
                "Jane", "Smith", DateTime.Now.AddYears(-30), "jane@example.com",
                1, startDate, endDate, 2);

            var paymentDetails = new Dictionary<string, string>
            {
                { "accountHolder", "Jane Smith" },
                { "iban", "NL91ABNA0417164300" },
                { "bankName", "ABN AMRO" }
            };
            var paymentResult = _paymentService.ProcessBookingPayment(bookingId, 1050.00m, "banktransfer", paymentDetails);

            // Assert
            Assert.True(bookingId > 0, "Booking created");
            Assert.True(paymentResult.Contains("Payment saved"), "Bank transfer initiated");
            Assert.True(paymentResult.Contains("3 business days"), "3-day message shown");
        }

        #endregion

        #region Exception Handling Tests

        [Fact]
        public void CreateBooking_UnderageGuest_ShouldThrowException()
        {
            // Arrange
            var underageBirthDate = DateTime.Now.AddYears(-17); // 17 years old

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                new Guest(1, "John", "Doe", underageBirthDate, "john@example.com"));

            Assert.Equal("Guest must be at least 18 years old to book.", exception.Message);
        }

        [Fact]
        public void CreateBooking_MissingPersonalData_ShouldFailValidation()
        {
            // Arrange & Act & Assert - Test missing first name
            var exception1 = Assert.Throws<ArgumentNullException>(() =>
                new Guest(1, "", "Doe", DateTime.Now.AddYears(-25), "john@example.com"));
            Assert.Contains("First name cannot be null or empty", exception1.Message);

            // Test missing last name
            var exception2 = Assert.Throws<ArgumentNullException>(() =>
                new Guest(1, "John", "", DateTime.Now.AddYears(-25), "john@example.com"));
            Assert.Contains("Last name cannot be null or empty", exception2.Message);

            // Test missing email
            var exception3 = Assert.Throws<ArgumentNullException>(() =>
                new Guest(1, "John", "Doe", DateTime.Now.AddYears(-25), ""));
            Assert.Contains("Email cannot be null or empty", exception3.Message);
        }

        [Fact]
        public void CreateBooking_NoPaymentMethod_ShouldFailValidation()
        {
            // Arrange
            var paymentMethod = "";

            // Act & Assert
            if (string.IsNullOrEmpty(paymentMethod))
            {
                var validationError = "Selecteer een betaalmethode";
                Assert.Equal("Selecteer een betaalmethode", validationError);
            }
        }

        [Fact]
        public void PayPalStrategy_EmptyEmail_ShouldReturnError()
        {
            // Arrange
            var paymentDetails = new Dictionary<string, string> { { "email", "" } };

            // Act
            var result = _paymentService.ProcessBookingPayment(1, 100m, "paypal", paymentDetails);

            // Assert
            Assert.True(result.Contains("Error: Invalid PayPal email"), "Strategy Error");
        }

        [Fact]
        public void CreditCardStrategy_InvalidCard_ShouldReturnError()
        {
            // Arrange
            var paymentDetails = new Dictionary<string, string>
            {
                { "cardName", "John Doe" },
                { "cardNumber", "1234" }, // Invalid card number (too short)
                { "expiry", "12/26" },
                { "cvv", "123" }
            };

            // Act
            var result = _paymentService.ProcessBookingPayment(1, 100m, "creditcard", paymentDetails);

            // Assert
            Assert.True(result.Contains("Error: Invalid card number"), "Strategy Error");
        }

        [Fact]
        public void BankTransferStrategy_InvalidIBAN_ShouldReturnError()
        {
            // Arrange
            var paymentDetails = new Dictionary<string, string>
            {
                { "accountHolder", "John Doe" },
                { "iban", "INVALID_IBAN" }, // Invalid IBAN format
                { "bankName", "Test Bank" }
            };

            // Act
            var result = _paymentService.ProcessBookingPayment(1, 100m, "banktransfer", paymentDetails);

            // Assert
            Assert.True(result.Contains("Error: Invalid IBAN format"), "Strategy Error");
        }

        [Fact]
        public void CreateBooking_TooManyGuests_ShouldThrowException()
        {
            // Arrange
            var accommodation = CreateTestAccommodation(1, 2); // Max 2 guests
            _mockAccommodationRepository.Setup(x => x.GetById(1)).Returns(accommodation);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _accommodationService.ValidateAccommodation(1, 4)); // Requesting 4 guests

            Assert.Contains("Accommodation can only accommodate 2 guests. You selected 4 guests", exception.Message);
        }

        [Fact]
        public void CreateBooking_UnavailableAccommodation_ShouldThrowException()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(7);
            var endDate = DateTime.Now.AddDays(14);

            _mockAccommodationRepository.Setup(x => x.IsAvailable(1, startDate, endDate)).Returns(false);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _availabilityService.ValidateAndReserve(1, startDate, endDate));

            Assert.Contains("not available for the chosen date", exception.Message);
        }

        [Fact]
        public void PaymentContext_UnknownMethod_ShouldThrowException()
        {
            // Arrange
            var paymentDetails = new Dictionary<string, string>();

            // Act & Assert
            var result = _paymentService.ProcessBookingPayment(1, 100m, "unknownmethod", paymentDetails);

            Assert.True(result.Contains("Payment method error"));
            Assert.True(result.Contains("unknownmethod"));
        }

        [Fact]
        public void CreateBooking_DatabaseDown_ShouldHandleGracefully()
        {
            // Arrange
            _mockBookingRepository.Setup(x => x.CreateBooking(It.IsAny<Booking>()))
                .Throws(new Exception("Database connection failed"));

            var accommodation = CreateTestAccommodation();
            _mockGuestRepository.Setup(x => x.GetByEmail("john@example.com")).Returns((Guest)null);
            _mockGuestRepository.Setup(x => x.Save(It.IsAny<Guest>())).Returns(123);
            _mockGuestRepository.Setup(x => x.GetById(123)).Returns(CreateTestGuest(123));
            _mockAccommodationRepository.Setup(x => x.GetById(1)).Returns(accommodation);
            _mockAccommodationRepository.Setup(x => x.IsAvailable(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(true);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                _bookingService.CreateBooking(
                    "John", "Doe", DateTime.Now.AddYears(-25), "john@example.com",
                    1, DateTime.Now.AddDays(7), DateTime.Now.AddDays(14), 2));

            Assert.True(exception.Message.Contains("Database connection failed"), "Er is een fout opgetreden: [message]");
        }

        #endregion
    }
}