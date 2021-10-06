using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandroidWorxApp
{
    public class Interop
    {
        private readonly IJSRuntime _jsRuntime;

        public Interop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetElementByName(string name)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>(
                    "interop.getElementByName",
                    name);
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task SubmitForm(string path, object fields)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "interop.submitForm",
                    path, fields);
            }
            catch
            {
            }
        }

        public async Task ShowSwallAlert(string type, string title, string message, string confirmBtnText, string cancelBtnText, object callback)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "interop.showSwallAlert",
                    type, title, message, confirmBtnText, cancelBtnText, callback);
            }
            catch
            {
            }
        }

        public async Task Overlay(bool show, object callback = null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    show ? "interop.showOverlay" : "interop.hideOverlay",
                    callback);
            }
            catch
            {
            }
        }

        public async Task RenderPickers(string className)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "interop.renderPickers",
                    className);
            }
            catch
            {
            }
        }

        public async Task WriteCookieAsync(string name, string value, int days)
        {
            try
            {
                await _jsRuntime.InvokeAsync<object>("interop.writeCookie", name, value, days);
            }
            catch (Exception)
            {
            }
        }

        public async Task<string> ReadCookies(string value)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("interop.readCookie", value);      
            }
            catch (Exception)
            {
                return null;
            }
       }
    }

    public class CallbackerResponse
    {
        public string[] arguments { get; private set; }
        public CallbackerResponse(string[] arguments)
        {
            this.arguments = arguments;
        }
        public T GetArg<T>(int i)
        {
            return JsonConvert.DeserializeObject<T>(arguments[i]);
        }
    }

    public class Callbacker
    {
        private IJSRuntime _js = null;
        private DotNetObjectReference<Callbacker> _this = null;
        private Dictionary<string, Action<string[]>> _callbacks = new Dictionary<string, Action<string[]>>();

        public Callbacker(IJSRuntime JSRuntime)
        {
            _js = JSRuntime;
            _this = DotNetObjectReference.Create(this);
        }

        [JSInvokable]
        public void _Callback(string callbackId, string[] arguments)
        {
            if (_callbacks.TryGetValue(callbackId, out Action<string[]> callback))
            {
                _callbacks.Remove(callbackId);
                callback(arguments);
            }
        }

        public Task<CallbackerResponse> InvokeJS(string cmd, params object[] args)
        {
            var t = new TaskCompletionSource<CallbackerResponse>();
            _InvokeJS((string[] arguments) => {
                t.TrySetResult(new CallbackerResponse(arguments));
            }, cmd, args);
            return t.Task;
        }

        public void InvokeJS(Action<CallbackerResponse> callback, string cmd, params object[] args)
        {
            _InvokeJS((string[] arguments) => {
                callback(new CallbackerResponse(arguments));
            }, cmd, args);
        }

        private void _InvokeJS(Action<string[]> callback, string cmd, object[] args)
        {
            string callbackId;
            do
            {
                callbackId = Guid.NewGuid().ToString();
            } while (_callbacks.ContainsKey(callbackId));
            _callbacks[callbackId] = callback;
            _js.InvokeVoidAsync("interop.callbacker", _this, "_Callback", callbackId, cmd, JsonConvert.SerializeObject(args));
        }
    }
}
