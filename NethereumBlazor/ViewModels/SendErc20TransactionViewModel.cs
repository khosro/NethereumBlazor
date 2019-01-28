﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using NethereumBlazor.Messages;
using NethereumBlazor.Services;
using ReactiveUI;

namespace NethereumBlazor.ViewModels
{
    public class SendErc20TransactionViewModel : SendTransactionBaseViewModel
    {
        private string _contractAddress;

        public string ContractAddress
        {
            get => _contractAddress;
            set => this.RaiseAndSetIfChanged(ref _contractAddress, value);
        }

        private string _transferTo;

        public string TransferTo
        {
            get => _transferTo;
            set => this.RaiseAndSetIfChanged(ref _transferTo, value);
        }

        private decimal _tokenAmount;

        public decimal TokenAmount
        {
            get => _tokenAmount;
            set => this.RaiseAndSetIfChanged(ref _tokenAmount, value);
        }


        private decimal _tokenBalance;

        public decimal TokenBalance
        {
            get => _tokenBalance;
            set => this.RaiseAndSetIfChanged(ref _tokenBalance, value);
        }

        private int _decimalPlaces;

        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set => this.RaiseAndSetIfChanged(ref _decimalPlaces, value);
        }


        public SendErc20TransactionViewModel(IAccountsService accountsService) : base(accountsService)
        {
            _decimalPlaces = 18;

            this.WhenAnyValue(x => x.Account, x => x.ContractAddress, (x,y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y)).Subscribe(async _ =>
                await RefreshTokenBalanceAsync()
            );

            MessageBus.Current.Listen<UrlChanged>().Subscribe(async x => { await RefreshTokenBalanceAsync(); });
        }

        public async Task RefreshTokenBalanceAsync()
        {
            if (!string.IsNullOrWhiteSpace(Account) && !string.IsNullOrWhiteSpace(ContractAddress))
            {
                TokenBalance = await AccountsService.GetAccountTokenBalanceAsync(Account, ContractAddress, DecimalPlaces);
            }
        }

        public async Task<string> SendTokenAsync()
        {
            var transferFunction =
                new TransferFunction()
                {
                    AmountToSend = new HexBigInteger(Web3.Convert.ToWei(AmountInEther)),
                    To = TransferTo,
                    FromAddress = Account,
                    Value = Web3.Convert.ToWei(TokenAmount, DecimalPlaces)
                };
            if (Gas != null)
                transferFunction.Gas = new HexBigInteger(Gas.Value);
            if (!string.IsNullOrEmpty(GasPrice))
            {
                var parsed = decimal.Parse(GasPrice);
                transferFunction.GasPrice = new HexBigInteger(Web3.Convert.ToWei(GasPrice, UnitConversion.EthUnit.Gwei));
            }

            if (Nonce != null)
                transferFunction.Nonce = new HexBigInteger(Nonce.Value);

            
            return await AccountsService.SendTransactionAsync(ContractAddress, transferFunction).ConfigureAwait(false);
        }
    }


   
}