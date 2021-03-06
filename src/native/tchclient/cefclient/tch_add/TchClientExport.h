﻿#ifndef CEF_TESTS_CEFCLIENT_TCH_ADD_EXPORT_H_
#define CEF_TESTS_CEFCLIENT_TCH_ADD_EXPORT_H_
#pragma once
#include "TchResourceHandler.h"
#include "TchQueryHandler.h"
#include "TchErrorHandler.h"
#include "TchWindowApi.h"

using namespace Tnelab;

#if defined(OS_LINUX)
#define TCHAPI  __attribute__((visibility("default"))) 
#elif defined(OS_WIN)
#define TCHAPI _declspec(dllexport)
#endif

extern "C"{
	TCHAPI int TchStart(const TchAppStartSettings start_settings);
	TCHAPI int SetTchErrorDelegate(TchErrorHandler::TchErrorDelegate* delegate);
	TCHAPI void SetJsInvokeDelegate(TchQueryHandler::JsInvokeDelegate delegate);
	TCHAPI void SetResourceRequestDelegate(TchResourceHandler::ResourceRequestDelegate delegate);
	TCHAPI void OnTchError(int code, const char* msg);
	TCHAPI void SetTchAppDomainName(const char* domain_name);
}
#endif  // CEF_TESTS_CEFCLIENT_TCH_ADD_EXPORT_H_
