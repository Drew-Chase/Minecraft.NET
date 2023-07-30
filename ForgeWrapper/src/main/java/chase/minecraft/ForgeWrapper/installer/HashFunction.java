package chase.minecraft.ForgeWrapper.installer;

import java.math.BigInteger;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Locale;

public enum HashFunction {
  MD5("md5", 32),
  SHA1("SHA-1", 40),
  SHA256("SHA-256", 64);
  
  private String algo;
  
  private String pad;
  
  HashFunction(String algo, int length) {
    this.algo = algo;
    this.pad = String.format(Locale.ENGLISH, "%0" + length + "d", new Object[] { Integer.valueOf(0) });
  }
  
  public MessageDigest get() {
    try {
      return MessageDigest.getInstance(this.algo);
    } catch (NoSuchAlgorithmException e) {
      throw new RuntimeException(e);
    } 
  }
  
  public String hash(byte[] data) {
    return pad((new BigInteger(1, get().digest(data))).toString(16));
  }
  
  public String pad(String hash) {
    return (this.pad + hash).substring(hash.length());
  }
}
