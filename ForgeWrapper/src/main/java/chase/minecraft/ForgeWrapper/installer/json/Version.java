package chase.minecraft.ForgeWrapper.installer.json;

import java.util.Map;

public class Version {
  private Map<String, Download> downloads;
  
  private Library[] libraries;
  
  public Download getDownload(String key) {
    return (this.downloads == null) ? null : this.downloads.get(key);
  }
  
  public Library[] getLibraries() {
    return (this.libraries == null) ? new Library[0] : this.libraries;
  }
  
  public static class Download {
    private String sha1;
    
    private String url;
    
    private boolean provided = false;
    
    public String getSha1() {
      return this.sha1;
    }
    
    public String getUrl() {
      return (this.url == null || this.provided) ? "" : this.url;
    }
  }
  
  public static class LibraryDownload extends Download {
    private String path;
    
    public String getPath() {
      return this.path;
    }
    
    public void setPath(String value) {
      this.path = value;
    }
  }
  
  public static class Library {
    private Artifact name;
    
    private Version.Downloads downloads;
    
    public Artifact getName() {
      return this.name;
    }
    
    public Version.Downloads getDownloads() {
      return this.downloads;
    }
  }
  
  public static class Downloads {
    private Version.LibraryDownload artifact;
    
    public Version.LibraryDownload getArtifact() {
      return this.artifact;
    }
  }
}
