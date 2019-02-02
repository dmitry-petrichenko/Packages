using System;
using ID5D6AAC.Common.EventDispatcher;
using Xunit;

namespace EventDispatcherTests
{
    public class EventDispatcherUnitTests
    {
        private const string SOME_EVENT = "SomeEvent";
        private const string SOME_OTHER_EVENT = "SomeOtherEvent";
        
        [Fact]
        public void ShouldCallEventHandlerOnEvent()
        {
            // Arrange
            bool called = false;
            var action = new Action(() => { called = true; });
            var eventDispatcher = new EventDispatcher();
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            
            // Act
            eventDispatcher.DispatchEvent(SOME_EVENT);
            
            // Assert
            Assert.True(called);
        }
        
        [Theory]
        [InlineData(3)]
        [InlineData(50)]
        [InlineData(8)]
        public void ShouldCallEventHandlerWithParameter(int expectedValue)
        {
            // Arrange
            int actualValue = 0;
            var action = new Action<int>(i => { actualValue = i; });
            var eventDispatcher = new EventDispatcher();
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            
            // Act
            eventDispatcher.DispatchEvent(SOME_EVENT, expectedValue);
            
            // Assert
            Assert.True(actualValue ==  expectedValue);
        }
        
        [Theory]
        [InlineData(true, "")]
        [InlineData(false, "test text")]
        public void ShouldCorrectUnsubscribeEventHandlerWithParameter(bool unsubscribe, string expectedValue)
        {
            // Arrange
            string actualValue = "";
            var action = new Action<string>(s => { actualValue = s; });
            var eventDispatcher = new EventDispatcher();
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            if (unsubscribe)
            {
                eventDispatcher.RemoveEventListener(SOME_EVENT, action);
                eventDispatcher.RemoveEventListener(SOME_EVENT, action);
            }

            // Act
            eventDispatcher.DispatchEvent(SOME_EVENT, expectedValue);
            
            // Assert
            Assert.True(actualValue ==  expectedValue);
        }
        
        [Fact]
        public void ShouldNotCallEventHandlerOnRemoveListener()
        {
            // Arrange
            bool called = false;
            var action = new Action(() => { called = true; });
            var eventDispatcher = new EventDispatcher();
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            eventDispatcher.RemoveEventListener(SOME_EVENT, action);
            
            // Act
            eventDispatcher.DispatchEvent(SOME_EVENT);
            
            // Assert
            Assert.False(called);
        }
        
        [Fact]
        public void ShouldSubscribeOnDifferentEvents()
        {
            // Arrange
            int actualCallCount = 0;
            int expectedCallCount = 2;
            var action = new Action(() => { actualCallCount++; });
            var eventDispatcher = new EventDispatcher();
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            eventDispatcher.AddEventListener(SOME_OTHER_EVENT, action);
            
            // Act
            eventDispatcher.DispatchEvent(SOME_EVENT);
            eventDispatcher.DispatchEvent(SOME_OTHER_EVENT);
            
            // Assert
            Assert.True(actualCallCount == expectedCallCount);
        }
        
        [Fact]
        public void ShouldCallEventHandlerOnceOnSeveralSubscribtions()
        {
            // Arrange
            int actualCallCount = 0;
            int expectedCallCount = 1;
            var action = new Action(() => { actualCallCount++; });
            var eventDispatcher = new EventDispatcher();
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            eventDispatcher.AddEventListener(SOME_EVENT, action);
            
            // Act
            eventDispatcher.DispatchEvent(SOME_EVENT);
            
            // Assert
            Assert.True(actualCallCount == expectedCallCount);
        }
    }
}