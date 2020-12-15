using System;
using System.Threading.Tasks;
using Xunit;

namespace C8F2740A.Common.ExecutionStrategies.Tests
{
    public class SafeExecutionTests
    {
        [Fact]
        public void TryCatchWithResultAsync_OnException_ShouldCatch()
        {
            var wasCatched = false;
            async Task<bool> TestMethod()
            {
                throw new Exception("test exception");
            }

            var task = SafeExecution.TryCatchWithResultAsync(() => TestMethod(), exception => wasCatched = true);
            
            Assert.True(wasCatched);
            Assert.False(task.Result);
        }
        
        [Fact]
        public void TryCatchWithResultAsync_OnExceptionAndNotAsyncSignature_ShouldCatch()
        {
            var wasCatched = false;
            Task<bool> TestMethod()
            {
                throw new Exception("test exception");
            }

            var task = SafeExecution.TryCatchWithResultAsync(() => TestMethod(), exception => wasCatched = true);
            
            Assert.True(wasCatched);
            Assert.False(task.Result);
        }
        
        [Fact]
        public void TryCatchWithResultAsync_OnSuccess_ShouldReturnResult()
        {
            var wasCatched = false;
            async Task<bool> TestMethod()
            {
                return true;
            }

            var task = SafeExecution.TryCatchWithResultAsync(() => TestMethod(), exception => wasCatched = true);
            
            Assert.False(wasCatched);
            Assert.True(task.Result);
        }
        
        [Fact]
        public void TryCatchWithResult_OnSuccess_ShouldReturnResult()
        {
            var wasCatched = false;
            bool TestMethod()
            {
                return true;
            }

            var result = SafeExecution.TryCatchWithResult(TestMethod, exception => wasCatched = true);
            
            Assert.False(wasCatched);
            Assert.True(result);
        }
    }
}