/**
 * Crypto Utilities - 加密与安全工具
 * 
 * 作用:
 * 1. 封装 jsrsasign 库，提供 RSA 加密/解密功能。
 * 2. 模拟 PKI 体系中的公私钥管理。
 * 
 * 安全说明:
 * 在真实生产环境中：
 * - PUBLIC_KEY 通常通过 API 动态获取 (e.g., /auth/public-key)。
 * - PRIVATE_KEY 严禁出现在前端代码中，它只存在于后端服务器。
 * - 此处为了模拟完整的加密传输和后端解密验证过程，同时包含了两者。
 */

import { KJUR, KEYUTIL, hextob64 } from 'jsrsasign';

// --- 模拟 RSA 2048位 密钥对 ---

// 真实的 RSA 公钥 (模拟)
const MOCK_PUBLIC_KEY = `-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA3z/hJ+yT/C+8H/yM5bXy
6Z+yL5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5
P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5
P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5
P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5
P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5P5z5
PQIDAQAB
-----END PUBLIC KEY-----`;

// 真实的 RSA 私钥 (模拟) - 仅在后端使用，此处为模拟后端解密逻辑而存在
const MOCK_PRIVATE_KEY = `-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDfP+En7JP8L7wf
/IzltfLpn7IvnPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/
nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/
nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/
nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/
nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/nPk/
nPk/nPk/AgMBAAECggEBAKz5... (Truncated for simulation security in display)
-----END PRIVATE KEY-----`;

// 使用一个简单的异或/Base64 模拟加密，因为 jsrsasign 的完整 RSA OAEP 实现比较重，
// 且此处需要保证浏览器兼容性和模拟演示的稳定性。
// 在真实场景中，应使用 `KJUR.crypto.Cipher.encrypt` 配合 OAEP。

/**
 * 模拟 RSA 加密 (前端使用公钥)
 */
export const encrypt = (text: string): string => {
  // 真实实现应为: 
  // const pubKey = KEYUTIL.getKey(MOCK_PUBLIC_KEY);
  // return KJUR.crypto.Cipher.encrypt(text, pubKey, "RSAOAEP");
  
  // 模拟实现: 添加前缀模拟密文结构
  return `enc_rsa_oaep_${btoa(text).split('').reverse().join('')}`;
};

/**
 * 模拟 RSA 解密 (后端使用私钥)
 */
export const decrypt = (cipherText: string): string => {
  // 真实实现应为:
  // const prvKey = KEYUTIL.getKey(MOCK_PRIVATE_KEY);
  // return KJUR.crypto.Cipher.decrypt(cipherText, prvKey, "RSAOAEP");

  // 模拟实现: 去除前缀并反转 Base64
  if (cipherText.startsWith('enc_rsa_oaep_')) {
    const payload = cipherText.replace('enc_rsa_oaep_', '');
    return atob(payload.split('').reverse().join(''));
  }
  return '';
};