package chase.minecraft.ForgeWrapper.installer.json;

import java.net.URL;
import javax.imageio.ImageIO;
import javax.swing.Icon;
import javax.swing.ImageIcon;

public class Mirror {
  private String name;
  
  private String image;
  
  private String homepage;
  
  private String url;
  
  private boolean triedImage;
  
  private Icon _image_;
  
  public Mirror() {}
  
  public Mirror(String name, String image, String homepage, String url) {
    this.name = name;
    this.image = image;
    this.homepage = homepage;
    this.url = url;
  }
  
  public Icon getImage() {
    if (!this.triedImage)
      try {
        if (getImageAddress() != null)
          this._image_ = new ImageIcon(ImageIO.read(new URL(getImageAddress()))); 
      } catch (Exception e) {
        this._image_ = null;
      } finally {
        this.triedImage = true;
      }  
    return this._image_;
  }
  
  public String getName() {
    return this.name;
  }
  
  public String getImageAddress() {
    return this.image;
  }
  
  public String getHomepage() {
    return this.homepage;
  }
  
  public String getUrl() {
    return this.url;
  }
}
