package chase.minecraft.ForgeWrapper.installer.json;

import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonNull;
import com.google.gson.JsonParseException;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;
import java.io.File;
import java.lang.reflect.Type;

public class Artifact {
  private String domain;
  
  private String name;
  
  private String version;
  
  private String classifier = null;
  
  private String ext = "jar";
  
  private String path;
  
  private String file;
  
  private String descriptor;
  
  public static Artifact from(String descriptor) {
    Artifact ret = new Artifact();
    ret.descriptor = descriptor;
    String[] pts = descriptor.split(":");
    ret.domain = pts[0];
    ret.name = pts[1];
    int last = pts.length - 1;
    int idx = pts[last].indexOf('@');
    if (idx != -1) {
      ret.ext = pts[last].substring(idx + 1);
      pts[last] = pts[last].substring(0, idx);
    } 
    ret.version = pts[2];
    if (pts.length > 3)
      ret.classifier = pts[3]; 
    ret.file = ret.name + '-' + ret.version;
    if (ret.classifier != null)
      ret.file += '-' + ret.classifier; 
    ret.file += '.' + ret.ext;
    ret.path = ret.domain.replace('.', '/') + '/' + ret.name + '/' + ret.version + '/' + ret.file;
    return ret;
  }
  
  public File getLocalPath(File base) {
    return new File(base, this.path.replace('/', File.separatorChar));
  }
  
  public String getDescriptor() {
    return this.descriptor;
  }
  
  public String getPath() {
    return this.path;
  }
  
  public String getFilename() {
    return this.file;
  }
  
  public String toString() {
    return getDescriptor();
  }
  
  public static class Adapter implements JsonDeserializer<Artifact>, JsonSerializer<Artifact> {
    public JsonElement serialize(Artifact src, Type typeOfSrc, JsonSerializationContext context) {
      return (src == null) ? (JsonElement)JsonNull.INSTANCE : (JsonElement)new JsonPrimitive(src.getDescriptor());
    }
    
    public Artifact deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context) throws JsonParseException {
      return json.isJsonPrimitive() ? Artifact.from(json.getAsJsonPrimitive().getAsString()) : null;
    }
  }
}
