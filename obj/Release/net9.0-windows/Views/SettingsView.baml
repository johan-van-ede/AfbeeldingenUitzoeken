   M S B A M L   `   `   `  ���� P  LAfbeeldingenUitzoeken, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:AfbeeldingenUitzoeken.ViewsAfbeeldingenUitzoeken.Views  =#clr-namespace:AfbeeldingenUitzoekenAfbeeldingenUitzoeken  S.clr-namespace:AfbeeldingenUitzoeken.ViewModels AfbeeldingenUitzoeken.ViewModels  .    (AfbeeldingenUitzoeken.Views.SettingsView   W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ;local)clr-namespace:AfbeeldingenUitzoeken.Views     3app#clr-namespace:AfbeeldingenUitzoeken     E
viewModels.clr-namespace:AfbeeldingenUitzoeken.ViewModels       � Title$  Settings��$	��450��$	��500�����#1E1E1E � 	Resources     BooleanToVisibilityConverter  DarkBackground �� Color  DarkSecondary  
DarkBorder  DarkText  GreenAccent  
BlueAccent  WarningColor  WarningIconStyle �� 
TargetType �� Property �� Value 	 UnifiedButtonStyle �� 
TargetType ��Name �� CornerRadius	 �� Triggers
 P� Property P� Value3  -System.Windows.Media.Effects.DropShadowEffect   Color   
BlurRadius   ShadowDepth   Opacity �� ScaleX �� ScaleY 
 DarkTextBoxStyle��%r  &	        &	      &	      &	 +     &	 >     &	 Q     &	 d     &	 w     &	 �     7  &		 O    &	
 u    �� �� $ #1E1E1E���� $ #2D2D2D���� $ #3E3E3E���� $ #E0E0E0���� $ #5CB85C���� $ #5BC0DE���� $ #FF4444���� " ��.���  � ��8   ��  � ��$ 16(���  � ��$ Center=���  � ��$ 5,0,0,0q���  �@�ToolTip$" This folder no longer exists����  � ��$ Segoe UI Symbol)��� " ��.���  � �� �#3E3E3E��  � �� �#E0E0E0��  � ��$
 10,5q���  � ��$ 0q���  �@�Cursor$
 Hand����  � �� �� " ��.R��� $ border��#��z��$ 5��$��0q�._��� $��Center=�$��Center=�	 P� 
 �@J�IsMouseOver$
 True��. ���  � �� �#5BC0DE��  �@J�Effect   $ Black��$ 10Q�$ 1Q�$	 0.7Q���  �@J�Opacity$	 0.9Q�P� 
 � ��$
 True��. ���  � z� �� $
 0.98Q�$
 0.98Q�P� 
 � {�$ False��. ���  �@J�Opacity$	 0.5Q��� " ��.���  � �� �#2D2D2D��  � �� �#E0E0E0��  � �� �#3E3E3E��  � ��$ 1q���  � ��$ 5q� � DataContext 8   2AfbeeldingenUitzoeken.ViewModels.SettingsViewModel  .��� #��[ .+��� $��20q�.+��� $2��,Source Library (Uit te zoeken afbeeldingen):��#��[ $��0,0,0,5q�� $��0,0,0,20q� � ColumnDefinitions �� $��* ��� $
��Auto ��� $
��Auto �.+���  �� 
IsReadOnly$
 True��#��[
 $��0������ *LibraryPath+�� $��1��#��[	 ���#3E3E3E$��40��$��5,0,0,0q� � ToolTip$ Select folder������ *SelectLibraryCommand+.���� $
��📂��$��14(��� $	��⚠��$��2��#��[ y��� *IsLibraryPathInvalid+ �� 	Converter# [  �� $��Keep Folder:��#��[ $��0,0,0,5q�� $��0,0,0,20q� �� $��* ��� $
��Auto ��� $
��Auto �.+��� $
 True��#��[
 $��0������ *KeepFolderPath+�� $��1��#��[	 ���#3E3E3E$��40��$��5,0,0,0q�$ Select folder������ *SelectKeepFolderCommand+.���� $
��📂��$��14(��� $	��⚠��$��2��#��[ y��� *IsKeepFolderPathInvalid+# [  �� $��Bin Folder:��#��[ $��0,0,0,5q�� $��0,0,0,20q� �� $��* ��� $
��Auto ��� $
��Auto �.+��� $
 True��#��[
 $��0������ *BinFolderPath+�� $��1��#��[	 ���#3E3E3E$��40��$��5,0,0,0q�$ Select folder������ *SelectBinFolderCommand+.���� $
��📂��$��14(��� $	��⚠��$��2��#��[ y��� *IsBinFolderPathInvalid+# [  �� $��Check Later Folder:��#��[ $��0,0,0,5q�� $��0,0,0,20q� �� $��* ��� $
��Auto ��� $
��Auto �.+��� $
 True��#��[
 $��0������ *CheckLaterFolderPath+�� $��1��#��[	 ���#3E3E3E$��40��$��5,0,0,0q�$ Select folder������ *SelectCheckLaterFolderCommand+.���� $
��📂��$��14(��� $	��⚠��$��2��#��[ y��� *IsCheckLaterFolderPathInvalid+# [  �� $
��Save��$	��120��$��35��#��[ $��Right=�$��0,20,0,0q�#��[	 ���� *SaveCommand+y��� *IsSaveButtonVisible+# [  �� #��[  J� Opacity$	 0.6Q�$��11(�$��Right=�$��0,10,0,0q����� *VersionInfo+