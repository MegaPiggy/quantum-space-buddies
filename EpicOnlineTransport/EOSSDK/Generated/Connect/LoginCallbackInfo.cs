// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Connect
{
	/// <summary>
	/// Output parameters for the <see cref="ConnectInterface.Login" /> function.
	/// </summary>
	public class LoginCallbackInfo : ICallbackInfo, ISettable
	{
		/// <summary>
		/// The <see cref="Result" /> code for the operation. <see cref="Result.Success" /> indicates that the operation succeeded; other codes indicate errors.
		/// </summary>
		public Result ResultCode { get; private set; }

		/// <summary>
		/// Context that was passed into <see cref="ConnectInterface.Login" />.
		/// </summary>
		public object ClientData { get; private set; }

		/// <summary>
		/// If login was succesful, this is the Product User ID of the local player that logged in.
		/// </summary>
		public ProductUserId LocalUserId { get; private set; }

		/// <summary>
		/// If the user was not found with credentials passed into <see cref="ConnectInterface.Login" />,
		/// this continuance token can be passed to either <see cref="ConnectInterface.CreateUser" />
		/// or <see cref="ConnectInterface.LinkAccount" /> to continue the flow.
		/// </summary>
		public ContinuanceToken ContinuanceToken { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(LoginCallbackInfoInternal? other)
		{
			if (other != null)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				ContinuanceToken = other.Value.ContinuanceToken;
			}
		}

		public void Set(object other)
		{
			Set(other as LoginCallbackInfoInternal?);
		}
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct LoginCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;
		private System.IntPtr m_ClientData;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_ContinuanceToken;

		public Result ResultCode
		{
			get
			{
				return m_ResultCode;
			}
		}

		public object ClientData
		{
			get
			{
				object value;
				Helper.TryMarshalGet(m_ClientData, out value);
				return value;
			}
		}

		public System.IntPtr ClientDataAddress
		{
			get
			{
				return m_ClientData;
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				ProductUserId value;
				Helper.TryMarshalGet(m_LocalUserId, out value);
				return value;
			}
		}

		public ContinuanceToken ContinuanceToken
		{
			get
			{
				ContinuanceToken value;
				Helper.TryMarshalGet(m_ContinuanceToken, out value);
				return value;
			}
		}
	}
}