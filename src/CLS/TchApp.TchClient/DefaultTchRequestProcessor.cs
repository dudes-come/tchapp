﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace TchApp.TchClient
{
    /// <summary>
    /// 默认web请求处理器和TchApp.TchClient.Client高度耦合
    /// </summary>
    public sealed class DefaultTchRequestProcessor
    {
        /// <summary>
        /// 从本地文件中构造web请求结果
        /// </summary>
        /// <param name="file_path">本地文件路径</param>
        /// <returns>未找到文件返回null</returns>
        public TchResponseInfo GetResponseInfoFormFile(TchRequestInfo tch_request_info)
        {
            var file_path = $".{tch_request_info.Uri.LocalPath}";
            if (!File.Exists(file_path)) return null;
            var raw_datas = File.ReadAllBytes(file_path);
            byte[] datas;
            //判断是否utf8签名
            if (raw_datas[0] == 239 && raw_datas[1] == 187 && raw_datas[2] == 191)
            {
                datas = new byte[raw_datas.Length - 3];
                for (int i = 0; i < datas.Length; ++i)
                {
                    datas[i] = raw_datas[i + 3];
                }
            }
            else
            {
                datas = raw_datas;
            }
            var mimeType = tch_request_info.Headers["Accept"].Split(';')[0].Split(',')[0];
            return TchHelper.ParseToResponse(datas, mime_type: mimeType);
        }
        /// <summary>
        /// 从TchApp.TchClient.Client对象的资源集合中构造web请求结果
        /// </summary>
        /// <param name="tch_request_info">web请求</param>
        /// <returns>未找到资源返回null</returns>
        public TchResponseInfo GetResponseInfoFormResource(TchRequestInfo tch_request_info)
        {
            //从程序集中查找
            string resource_name = tch_request_info.Uri.AbsolutePath.Remove(0, 1).Replace("/", ".");
            Stream resource_stream = null;
            foreach (var assembly in Application.This.ResourceAssemblySet)
            {
                resource_stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{resource_name}");
                if (resource_stream != null) break;
            }
            if (resource_stream != null)
            {
                using (resource_stream)
                {
                    byte[] bytes = new byte[resource_stream.Length];
                    resource_stream.Read(bytes, 0, bytes.Length);
                    var mimeType = tch_request_info.Headers["Accept"].Split(';')[0].Split(',')[0];
                    return TchHelper.ParseToResponse(bytes,mime_type:mimeType);
                }
            }
            return null;
        }
        /// <summary>
        /// 从TchApp.TchClient.Client对象过滤器中构造web请求结果
        /// </summary>
        /// <param name="url">要过滤的url</param>
        /// <returns></returns>
        public TchResponseInfo GetResponseInfoFormFilter(string url)
        {
            if (Application.This.FilterUrlDic.ContainsKey(url))
                return Application.This.FilterUrlDic[url](url);
            return null;
        }
        /// <summary>
        /// 封装后的web请求默认处理器
        /// </summary>
        /// <param name="tch_request_info">解析过的web请求数据</param>
        /// <returns></returns>
        public TchResponseInfo ProcessTchRequest(TchRequestInfo tch_request_info)
        {
            TchResponseInfo tch_response_info = this.GetResponseInfoFormFilter(tch_request_info.Uri.AbsolutePath);
            if (tch_response_info != null)
            {
                return tch_response_info;
            }

            tch_response_info = this.GetResponseInfoFormFile(tch_request_info);
            if (tch_response_info != null)
            {
                return tch_response_info;
            }

            tch_response_info = this.GetResponseInfoFormResource(tch_request_info);
            if (tch_response_info != null)
            {                
                return tch_response_info;
            }

            //未找到任何资源
            Application.This.OnError(-1, $"[{tch_request_info.Uri.AbsolutePath}] File Not Found");
            return TchHelper.ParseToResponse(Encoding.UTF8.GetBytes($"<html><head><meta charset=\"UTF-8\"/><title>处理请求错误</title></head><body><h1>{tch_request_info.Uri.AbsolutePath}没找到</h1></body></html>"), 404, "File Not Found");
        }
        private DefaultTchRequestProcessor() { }
        public readonly static DefaultTchRequestProcessor This = new DefaultTchRequestProcessor();
    }
}
