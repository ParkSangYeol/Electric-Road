// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("K6imqZkrqKOrK6ioqTiXq+UlXQBqHSbWUAX2pv7QK0RZGvOcbt4BXwHvm9e3xs29vRHAvDN8fGybJgsA2ZhPwCDzGjMKsvZD4sb4jZlh8A6aBeMa0KEaWZjPlbqKygSWC8VZcZkrqIuZpK+ggy/hL16kqKiorKmqVNvRDk26d2/jVbP+rTujbvxQYhTF08mv3HAMkGS+PujQ49+LGiZD3bs+u0xVMg30ZkKdt7ETpwBC5ScgbOrht3pAxVviks5Vbsm4hojDuMckroL7AteM9eI/+QvfKtslzg4cwAv9pJPhJIFVb5t8JJfG3Twg+rdhy/j4pfuNKQCRjpJiPGZwT0h+s5zf+CnnQ6RsvnU9tt3Z8D4fKtNGGSKJQf8mynTjmquqqKmo");
        private static int[] order = new int[] { 1,11,8,5,9,11,12,10,11,13,13,11,13,13,14 };
        private static int key = 169;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
