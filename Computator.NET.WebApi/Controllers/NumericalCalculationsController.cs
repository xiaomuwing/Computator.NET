﻿using System;
using System.Net;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes.Functions;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CalculationsMode = Computator.NET.DataTypes.CalculationsMode;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/numerical-calculations")]
    public class NumericalCalculationsController : Controller
    {
        private const uint ITERATIONS_MAX = 10000;
        private const double ERROR_MAX_MIN = 1e-8;

        private readonly IFunctionsProvider _functionsProvider;
        private readonly IModeDeterminer _modeDeterminer;
        private readonly ILogger<NumericalCalculationsController> _logger;

        public NumericalCalculationsController(IExpressionsEvaluator expressionsEvaluator, IModeDeterminer modeDeterminer, ILogger<NumericalCalculationsController> logger, IFunctionsProvider functionsProvider)
        {
            _modeDeterminer = modeDeterminer;
            _logger = logger;
            _functionsProvider = functionsProvider;
        }

        [HttpGet("integral/{method}/{equation}/{a}/{b}")]
        [HttpGet("integral/{method}/{equation}/{a}/{b}/{n}")]
        [HttpGet("integral/{method}/{equation}/{a}/{b}/{n}/{customFunctionsCode}")]
        public string GetRealIntegral(string method, string equation, double a, double b, double n=ITERATIONS_MAX, string customFunctionsCode="")
        {
            Decode(equation, method, customFunctionsCode, out string decodedEquation, out string decodedMethod, out string decodedCustomFunctionsCode);

            var func = _functionsProvider.GetFunction(decodedEquation, CalculationsMode.Real, decodedCustomFunctionsCode);

            var result = NumericalMethodsInfo.Instance.IntegrationMethods[decodedMethod].Invoke((double x) => func.Evaluate(x), a, b, n);
            var resultMathString = result.ToMathString();
            return resultMathString;
        }

        [HttpGet("derivative/{method}/{equation}/{x}")]
        [HttpGet("derivative/{method}/{equation}/{x}/{order}")]
        [HttpGet("derivative/{method}/{equation}/{x}/{order}/{epsilon}")]
        [HttpGet("derivative/{method}/{equation}/{x}/{order}/{epsilon}/{customFunctionsCode}")]
        public string GetRealDerrivative(string method, string equation, double x, uint order=1, double epsilon=ERROR_MAX_MIN, string customFunctionsCode = "")
        {
            Decode(equation, method, customFunctionsCode, out string decodedEquation, out string decodedMethod, out string decodedCustomFunctionsCode);

            var func = _functionsProvider.GetFunction(decodedEquation, CalculationsMode.Real, decodedCustomFunctionsCode);

            var result = NumericalMethodsInfo.Instance.DerrivationMethods[decodedMethod].Invoke((double val) => func.Evaluate(val), x, order, epsilon);
            var resultMathString = result.ToMathString();
            return resultMathString;
        }

        [HttpGet("function-root/{method}/{equation}/{a}/{b}")]
        [HttpGet("function-root/{method}/{equation}/{a}/{b}/{maxError}")]
        [HttpGet("function-root/{method}/{equation}/{a}/{b}/{maxError}/{maxIterations}")]
        [HttpGet("function-root/{method}/{equation}/{a}/{b}/{maxError}/{maxIterations}/{customFunctionsCode}")]
        public string GetRealFunctionRoot(string method, string equation, double a, double b, double maxError=ERROR_MAX_MIN, uint maxIterations=ITERATIONS_MAX, string customFunctionsCode = "")
        {
            Decode(equation, method, customFunctionsCode, out string decodedEquation, out string decodedMethod, out string decodedCustomFunctionsCode);

            var func = _functionsProvider.GetFunction(decodedEquation, CalculationsMode.Real, decodedCustomFunctionsCode);

            var result = NumericalMethodsInfo.Instance.FunctionRootMethods[decodedMethod].Invoke((double val) => func.Evaluate(val), a, b, maxError, maxIterations);
            var resultMathString = result.ToMathString();
            return resultMathString;
        }

        private void Decode(string equation, string method, string customFunctionsCode, out string decodedEquation, out string decodedMethod, out string decodedCustomFunctionsCode)
        {
            decodedEquation = WebUtility.UrlDecode(equation);
            _logger.LogInformation($"Decoded equation '{equation}' to '{decodedEquation}'");

            decodedMethod = WebUtility.UrlDecode(method);
            _logger.LogInformation($"Decoded method '{method}' to '{decodedMethod}'");

            decodedCustomFunctionsCode = WebUtility.UrlDecode(customFunctionsCode);
            _logger.LogInformation($"Decoded custom functions code '{customFunctionsCode}' to '{decodedCustomFunctionsCode}'");
        }
    }
}
