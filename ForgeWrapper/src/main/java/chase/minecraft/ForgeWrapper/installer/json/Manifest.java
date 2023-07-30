package chase.minecraft.ForgeWrapper.installer.json;

import java.util.List;

public class Manifest {
  private List<Info> versions;
  
  public String getUrl(String version) {
    return (this.versions == null) ? null : this.versions.stream().filter(v -> version.equals(v.getId())).map(Info::getUrl).findFirst().orElse(null);
  }
  
  public static class Info {
    private String id;
    
    private String url;
    
    public String getId() {
      return this.id;
    }
    
    public String getUrl() {
      return this.url;
    }
  }
}
