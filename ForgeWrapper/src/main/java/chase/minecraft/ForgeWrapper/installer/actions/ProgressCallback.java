package chase.minecraft.ForgeWrapper.installer.actions;

import java.io.IOException;
import java.io.OutputStream;

public interface ProgressCallback {
  public enum MessagePriority {
    LOW, NORMAL, HIGH;
  }
  
  default void start(String label) {
    message(label);
  }
  
  default void stage(String message) {
    message(message);
  }
  
  default void message(String message) {
    message(message, MessagePriority.NORMAL);
  }
  
  void message(String paramString, MessagePriority paramMessagePriority);
  
  default void progress(double progress) {}
  
  public static final ProgressCallback TO_STD_OUT = new ProgressCallback() {
      public void message(String message, ProgressCallback.MessagePriority priority) {
        System.out.println(message);
      }
    };
  
  static ProgressCallback withOutputs(OutputStream... streams) {
    return new ProgressCallback() {
        public void message(String message, ProgressCallback.MessagePriority priority) {
          message = message + System.lineSeparator();
          for (OutputStream out : streams) {
            try {
              out.write(message.getBytes());
              out.flush();
            } catch (IOException e) {
              throw new RuntimeException(e);
            } 
          } 
        }
      };
  }
}
