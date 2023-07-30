package chase.minecraft.ForgeWrapper.installer;

import java.io.InputStream;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.security.KeyStore;
import java.security.KeyStoreException;
import java.security.cert.Certificate;
import java.util.Collections;
import java.util.Map;
import java.util.stream.Collectors;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManagerFactory;

import chase.minecraft.ForgeWrapper.Main;
import chase.minecraft.ForgeWrapper.installer.actions.ProgressCallback;

class FixSSL {
  private static boolean hasJavaForDownload(ProgressCallback callback) {
    String javaVersion = System.getProperty("java.version");
    callback.message("Found java version " + javaVersion);
    if (javaVersion != null && javaVersion.startsWith("1.8.0_"))
      try {
        if (Integer.parseInt(javaVersion.substring("1.8.0_".length())) < 101)
          return false; 
      } catch (NumberFormatException e) {
        e.printStackTrace();
        callback.message("Could not parse java version!");
      }  
    return true;
  }
  
  static void fixup(ProgressCallback callback) {
    if (hasJavaForDownload(callback))
      return; 
    try {
      KeyStore keyStore = KeyStore.getInstance(KeyStore.getDefaultType());
      Path ksPath = Paths.get(System.getProperty("java.home"), new String[] { "lib", "security", "cacerts" });
      keyStore.load(Files.newInputStream(ksPath, new java.nio.file.OpenOption[0]), "changeit".toCharArray());
      Map<String, Certificate> jdkTrustStore = (Map<String, Certificate>)Collections.<String>list(keyStore.aliases()).stream().collect(Collectors.toMap(a -> a, alias -> {
              try {
                return keyStore.getCertificate(alias);
              } catch (KeyStoreException e) {
                throw new UncheckedKeyStoreException(e);
              } 
            }));
      KeyStore leKS = KeyStore.getInstance(KeyStore.getDefaultType());
      InputStream leKSFile = Main.getInstallerClassLoader().getResourceAsStream("lekeystore.jks");
      leKS.load(leKSFile, "supersecretpassword".toCharArray());
      Map<String, Certificate> leTrustStore = (Map<String, Certificate>)Collections.<String>list(leKS.aliases()).stream().collect(Collectors.toMap(a -> a, alias -> {
              try {
                return leKS.getCertificate(alias);
              } catch (KeyStoreException e) {
                throw new UncheckedKeyStoreException(e);
              } 
            }));
      KeyStore mergedTrustStore = KeyStore.getInstance(KeyStore.getDefaultType());
      mergedTrustStore.load(null, new char[0]);
      for (Map.Entry<String, Certificate> entry : jdkTrustStore.entrySet())
        mergedTrustStore.setCertificateEntry(entry.getKey(), entry.getValue()); 
      for (Map.Entry<String, Certificate> entry : leTrustStore.entrySet())
        mergedTrustStore.setCertificateEntry(entry.getKey(), entry.getValue()); 
      TrustManagerFactory instance = TrustManagerFactory.getInstance(TrustManagerFactory.getDefaultAlgorithm());
      instance.init(mergedTrustStore);
      SSLContext tls = SSLContext.getInstance("TLS");
      tls.init(null, instance.getTrustManagers(), null);
      HttpsURLConnection.setDefaultSSLSocketFactory(tls.getSocketFactory());
      callback.message("Added Lets Encrypt root certificates as additional trust");
    } catch (UncheckedKeyStoreException|KeyStoreException|java.io.IOException|java.security.NoSuchAlgorithmException|java.security.cert.CertificateException|java.security.KeyManagementException e) {
      callback.message("Failed to load lets encrypt certificate. Expect problems", ProgressCallback.MessagePriority.HIGH);
      e.printStackTrace();
    } 
  }
  
  private static class UncheckedKeyStoreException extends RuntimeException {
    public UncheckedKeyStoreException(Throwable cause) {
      super(cause);
    }
  }
}
