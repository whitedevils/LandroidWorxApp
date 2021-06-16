using Microsoft.JSInterop;
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
}
