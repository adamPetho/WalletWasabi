using System;
using System.Linq;
using Xunit;
using WalletWasabi.Fluent.QRCodeDecoder;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using WalletWasabi.Helpers;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace WalletWasabi.Tests.UnitTests.QrCode
{
	public class QrCodeDecodingTests
	{
		private readonly string _commonPartialPath = Path.Combine(EnvironmentHelpers.GetFullBaseDirectory(), "UnitTests", "QrCode", "QrTestResources");
		private readonly QRCodeDetector _qRCodeDetector = new();

		[Fact]
		public void GetCorrectAddressFromImage()
		{
			using var app = Start();
			string expectedAddress = "tb1ql27ya3gufs5h0ptgjhjd0tm52fq6q0xrav7xza";
			string otherExpectedAddress = "tb1qfas0k9rn8daqggu7wzp2yne9qdd5fr5wf2u478";

			string path = Path.Combine(_commonPartialPath, "AddressTest1.png");
			using var image = LoadBitmap(path);

			using IOutputArray points = new Mat();
			using IOutputArray straightQrCode = new Mat();

			_qRCodeDetector.Detect(image, points);
			var dataCollection = _qRCodeDetector.Decode(image, points, straightQrCode);

			Assert.NotNull(dataCollection);
			Assert.Equal(expectedAddress, dataCollection);

			string otherPath = Path.Combine(_commonPartialPath, "AddressTest2.png");
			using var image2 = LoadBitmap(otherPath);

			using IOutputArray points2 = new Mat();
			using IOutputArray straightQrCode2 = new Mat();

			_qRCodeDetector.Detect(image2, points2);
			var dataCollection2 = _qRCodeDetector.Decode(image2, points2, straightQrCode2);

			Assert.NotNull(dataCollection2);
			Assert.Equal(otherExpectedAddress, dataCollection2);

			Assert.NotEqual(dataCollection, dataCollection2);
		}

		[Fact]
		public void IncorrectImageThrowsException()
		{
			using var app = Start();

			string path = Path.Combine(_commonPartialPath, "NotBitcoinAddress.jpg");
			using var image = LoadBitmap(path);

			using IOutputArray points = new Mat();
			using IOutputArray straightQrCode = new Mat();

			bool result = _qRCodeDetector.Detect(image, points);
			Assert.False(result);

			Assert.Throws<CvException>(() => _qRCodeDetector.Decode(image, points, straightQrCode));
		}

		[Fact]
		public void DecodePictureTakenByPhone()
		{
			using var app = Start();
			string expectedAddress = "tb1qutgpgraaze3hqnvt2xyw5acsmd3urprk3ff27d";

			string path = Path.Combine(_commonPartialPath, "QrByPhone.jpg");
			using var image = LoadBitmap(path);

			using IOutputArray points = new Mat();
			using IOutputArray straightQrCode = new Mat();

			_qRCodeDetector.Detect(image, points);
			var dataCollection = _qRCodeDetector.Decode(image, points, straightQrCode);

			Assert.NotNull(dataCollection);
			Assert.Equal(expectedAddress, dataCollection);
		}

		/*
		[Fact]
		public void DecodeDifficultPictureTakenByPhone()
		{
			using var app = Start();
			QRDecoder decoder = new();
			string expectedOutput = "Top right corner";

			string path = Path.Combine(_commonPartialPath, "QRBrick.jpg");
			using var bmp = LoadBitmap(path);
			var dataCollection = decoder.SearchQrCodes(bmp);

			Assert.Single(dataCollection);
			Assert.Equal(expectedOutput, dataCollection.First());
		}

		[Fact]
		public void DecodePictureWithZebraBackground()
		{
			using var app = Start();
			QRDecoder decoder = new();
			string expectedOutput = "Let's see a Zebra.";

			string path = Path.Combine(_commonPartialPath, "QRwithZebraBackground.png");
			using var bmp = LoadBitmap(path);
			var dataCollection = decoder.SearchQrCodes(bmp);

			Assert.Single(dataCollection);
			Assert.Equal(expectedOutput, dataCollection.First());
		}

		[Fact]
		public void DecodePictureWithMouseOverTheQRCodeReturnsNull()
		{
			using var app = Start();
			QRDecoder decoder = new();
			string expectedOutput = "tb1qutgpgraaze3hqnvt2xyw5acsmd3urprk3ff27d";

			string path = Path.Combine(_commonPartialPath, "mouseOverQR.jpg");
			using var bmp = LoadBitmap(path);
			var dataCollection = decoder.SearchQrCodes(bmp);

			Assert.Single(dataCollection);
			Assert.Equal(expectedOutput, dataCollection.First());
		}

		[Fact]
		public void DecodePictureWithImageInsideTheQR()
		{
			using var app = Start();
			QRDecoder decoder = new();
			string expectedOutput = "bitcoin:bc1q3r0ayktdsh8yd3krk5zkvpc5weeqnmw8ztzsgx?amount=1000&label=GIMME%201000BTC";

			string path = Path.Combine(_commonPartialPath, "Payment_details_included.jpg");
			using var bmp = LoadBitmap(path);
			var dataCollection = decoder.SearchQrCodes(bmp);

			Assert.Equal(expectedOutput, dataCollection.First());
		}

		[Fact]
		public void DecodePictureWithLegacyAddress()
		{
			using var app = Start();
			QRDecoder decoder = new();
			string expectedOutput = "bitcoin:1EYTGtG4LnFfiMvjJdsU7GMGCQvsRSjYhx";

			string path = Path.Combine(_commonPartialPath, "Random_address_starting_with_1.png");
			using var bmp = LoadBitmap(path);
			var dataCollection = decoder.SearchQrCodes(bmp);

			Assert.Equal(expectedOutput, dataCollection.First());
		}

		[Fact]
		public void DecodeQrCodeInsideQrCode()
		{
			using var app = Start();
			QRDecoder decoder = new();
			string firstExpectedOutput = "Big QR Code";
			string secondExpectedOutput = "Small QR Code";
			int expectedLength = 2;

			// If there are two QR Codes on the picture, it will find both, even if they are
			// stacked on eachother, until the QR Code's quality is okay and is still readable
			string path = Path.Combine(_commonPartialPath, "DoubleQRCode.png");
			using var bmp = LoadBitmap(path);
			var dataCollection = decoder.SearchQrCodes(bmp).ToArray();

			Assert.Equal(expectedLength, dataCollection.Length);
			Assert.Equal(firstExpectedOutput, dataCollection[0]);
			Assert.Equal(secondExpectedOutput, dataCollection[1]);
		}
		*/

		private static Image<Rgb, byte> LoadBitmap(string path)
		{
			//using var fs = File.OpenRead(path);
			//return WriteableBitmap.Decode(fs);
			Image<Rgb, byte> img = new(path);
			return img;
		}

		private static IDisposable Start()
		{
			var scope = AvaloniaLocator.EnterScope();
			SkiaPlatform.Initialize();
			return scope;
		}
	}
}
