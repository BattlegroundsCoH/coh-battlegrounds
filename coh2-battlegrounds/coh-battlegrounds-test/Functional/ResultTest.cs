using Battlegrounds.Functional;

namespace Battlegrounds.Testing.Functional;

public class ResultTest {

    [Test]
    public void Success_Result_HasValue() {

        // Arrange
        var success = new Success<int>(42);

        // Act
        int? value = success.Value;

        // Assert
        Assert.Multiple(() => {
            Assert.That(success.IsEmpty, Is.EqualTo(false));
            Assert.That(value, Is.EqualTo(42));
        });

    }

    [Test]
    public void Success_Result_Then_Action_Executed() {

        // Arrange
        var success = new Success<string>("Hello");
        string? result = null;

        // Act
        success.Then(value => result = value);

        // Assert
        Assert.That(result, Is.EqualTo("Hello"));

    }

    [Test]
    public void Success_Result_Else_Action_NotExecuted() {

        // Arrange
        var success = new Success<int>(42);
        bool actionExecuted = false;

        // Act
        success.Else(() => actionExecuted = true);

        // Assert
        Assert.That(actionExecuted, Is.False);

    }

    [Test]
    public void Failure_Result_HasNoValue() {

        // Arrange
        var failure = new Failure<string>();

        // Assert
        Assert.That(failure.IsEmpty, Is.True);
        Assert.Throws<InvalidOperationException>(() => { var _ = failure.Value; });

    }

    [Test]
    public void Failure_Result_Then_Action_NotExecuted() {

        // Arrange
        var failure = new Failure<int>();
        bool actionExecuted = false;

        // Act
        failure.Then(value => actionExecuted = true);

        // Assert
        Assert.That(actionExecuted, Is.False);

    }

    [Test]
    public void Failure_Result_Else_Action_Executed() {

        // Arrange
        var failure = new Failure<string>();
        bool actionExecuted = false;

        // Act
        failure.Else(() => actionExecuted = true);

        // Assert
        Assert.That(actionExecuted, Is.True);

    }

    [Test]
    public void DeferredResult_Value_Available_Success() {

        // Arrange
        var provider = new AsyncResult<int>.AsyncResultProvider();
        var deferredResult = new AsyncResult<int>(provider);
        int? result = null;

        // Act
        deferredResult.Then(value => result = value);
        provider.Success(42);

        // Assert
        Assert.That(result, Is.EqualTo(42));

    }

    [Test]
    public void DeferredResult_Value_Available_Failure() {

        // Arrange
        var provider = new AsyncResult<int>.AsyncResultProvider();
        var deferredResult = new AsyncResult<int>(provider);
        bool actionExecuted = false;

        // Act
        deferredResult.Else(() => actionExecuted = true);
        provider.Failure();

        // Assert
        Assert.That(actionExecuted, Is.True);

    }

    [Test]
    public void DeferredResult_Value_NotAvailable() {

        // Arrange
        var provider = new AsyncResult<int>.AsyncResultProvider();
        var deferredResult = new AsyncResult<int>(provider);
        bool actionExecuted = false;

        // Act
        deferredResult.Else(() => actionExecuted = true);

        // Assert
        Assert.That(actionExecuted, Is.False);

    }

}
