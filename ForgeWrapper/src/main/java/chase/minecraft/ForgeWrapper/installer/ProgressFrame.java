package chase.minecraft.ForgeWrapper.installer;

import java.awt.Font;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.event.ActionEvent;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JProgressBar;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import chase.minecraft.ForgeWrapper.installer.actions.ProgressCallback;

public class ProgressFrame extends JFrame implements ProgressCallback {
  private final ProgressCallback parent;
  
  private final JPanel panel = new JPanel();
  
  private final JLabel progressText;
  
  private final JProgressBar progressBar;
  
  private final JTextArea consoleArea;
  
  public ProgressFrame(ProgressCallback parent, String title, Runnable canceler) {
    this.parent = parent;
    setResizable(false);
    setTitle(title);
    setDefaultCloseOperation(2);
    setBounds(100, 100, 600, 400);
    setContentPane(this.panel);
    setLocationRelativeTo(null);
    GridBagLayout gridBagLayout = new GridBagLayout();
    gridBagLayout.columnWidths = new int[] { 600, 0 };
    gridBagLayout.rowHeights = new int[] { 0, 0, 0, 200 };
    gridBagLayout.columnWeights = new double[] { 1.0D, Double.MIN_VALUE };
    gridBagLayout.rowWeights = new double[] { 0.0D, 0.0D, 0.0D, 1.0D };
    this.panel.setLayout(gridBagLayout);
    this.progressText = new JLabel("Progress Text");
    GridBagConstraints gbc_lblProgressText = new GridBagConstraints();
    gbc_lblProgressText.insets = new Insets(10, 0, 5, 0);
    gbc_lblProgressText.gridx = 0;
    gbc_lblProgressText.gridy = 0;
    this.panel.add(this.progressText, gbc_lblProgressText);
    this.progressBar = new JProgressBar();
    GridBagConstraints gbc_progressBar = new GridBagConstraints();
    gbc_progressBar.insets = new Insets(0, 25, 5, 25);
    gbc_progressBar.fill = 2;
    gbc_progressBar.gridx = 0;
    gbc_progressBar.gridy = 1;
    this.panel.add(this.progressBar, gbc_progressBar);
    JButton btnCancel = new JButton("Cancel");
    btnCancel.addActionListener(e -> {
          canceler.run();
          dispose();
        });
    GridBagConstraints gbc_btnCancel = new GridBagConstraints();
    gbc_btnCancel.insets = new Insets(0, 25, 5, 25);
    gbc_btnCancel.fill = 2;
    gbc_btnCancel.gridx = 0;
    gbc_btnCancel.gridy = 2;
    this.panel.add(btnCancel, gbc_btnCancel);
    this.consoleArea = new JTextArea();
    this.consoleArea.setFont(new Font("Monospaced", 0, 11));
    GridBagConstraints gbc_textArea = new GridBagConstraints();
    gbc_textArea.insets = new Insets(15, 25, 25, 25);
    gbc_textArea.fill = 1;
    gbc_textArea.gridx = 0;
    gbc_textArea.gridy = 3;
    JScrollPane scroll = new JScrollPane(this.consoleArea, 22, 30);
    scroll.setAutoscrolls(true);
    this.panel.add(scroll, gbc_textArea);
  }
  
  public void start(String label) {
    message(label, ProgressCallback.MessagePriority.HIGH, false);
    this.progressBar.setValue(0);
    this.progressBar.setIndeterminate(false);
    this.parent.start(label);
  }
  
  public void progress(double progress) {
    this.progressBar.setValue((int)(progress * 100.0D));
    this.parent.progress(progress);
  }
  
  public void stage(String message) {
    message(message, ProgressCallback.MessagePriority.HIGH, false);
    this.progressBar.setIndeterminate(true);
    this.parent.stage(message);
  }
  
  public void message(String message, ProgressCallback.MessagePriority priority) {
    message(message, priority, true);
  }
  
  public void message(String message, ProgressCallback.MessagePriority priority, boolean notifyParent) {
    if (priority == ProgressCallback.MessagePriority.HIGH)
      this.progressText.setText(message); 
    this.consoleArea.append(message + "\n");
    this.consoleArea.setCaretPosition(this.consoleArea.getDocument().getLength());
    if (notifyParent)
      this.parent.message(message, priority); 
  }
}
