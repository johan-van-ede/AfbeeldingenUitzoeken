   M S B A M L   `   `   `  ����P  LAfbeeldingenUitzoeken, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:AfbeeldingenUitzoeken.ViewsAfbeeldingenUitzoeken.Views  5   	   =#clr-namespace:AfbeeldingenUitzoekenAfbeeldingenUitzoeken  5   	   S.clr-namespace:AfbeeldingenUitzoeken.ViewModels AfbeeldingenUitzoeken.ViewModels  5   	   S.clr-namespace:AfbeeldingenUitzoeken.Converters AfbeeldingenUitzoeken.Converters  5	   	   .    (AfbeeldingenUitzoeken.Views.SettingsView   5   	   W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:AfbeeldingenUitzoeken.Views     5   	   3app#clr-namespace:AfbeeldingenUitzoeken     5   	   E
viewModels.clr-namespace:AfbeeldingenUitzoeken.ViewModels     5   	   E
converters.clr-namespace:AfbeeldingenUitzoeken.Converters     5	   	     � Title$  Settings��5   	   $	��450��6   $	��500��6'   ���#1E1E1E � 	Resources 5          BooleanToVisibilityConverter  DarkBackground �� Color  DarkSecondary  
DarkBorder  DarkText  GreenAccent  
BlueAccent  WarningColor  WarningIconStyle �� 
TargetType �� Property �� Value 	 UnifiedButtonStyle �� 
TargetType ��Name �� CornerRadius	 �� Triggers
 P� Property P� Value3  -System.Windows.Media.Effects.DropShadowEffect   Color   
BlurRadius   ShadowDepth   Opacity �� ScaleX �� ScaleY 
 DarkTextBoxStyle��%X  &	        &	      &	 4     &	 U     &	 v     &	 �     &	 �     &	 �     &	 �     7  &		 *    &	
 �    �� 5   
   6'   �� 5   
   $ #1E1E1E��61   �� 5   
   $ #2D2D2D��60   �� 5   
   $ #3E3E3E��6-   �� 5   
   $ #E0E0E0��6+   �� 5   
   $ #5CB85C��6.   �� 5   
   $ #5BC0DE��6-   �� 5   
   $ #FF4444��6/   �� 5   
   " ��6)   .�5      ��  � ��8   6+   �� 5       � ��$ 16(�6)   �� 5       � ��$ Center=�62   �� 5       � ��$ 5,0,0,0q�6'   �� 5       �@�ToolTip$" This folder no longer exists��6(   �� 5        � ��$ Segoe UI Symbol)�6+   5!      �� 5$   
   " ��6+   .�5%      ��  � �� �#3E3E3E6+   �� 5&       � �� �#E0E0E06+   �� 5'       � ��$
 10,5q�6(   �� 5(       � ��$ 0q�60   �� 5)       �@�Cursor$
 Hand��6'   �� 5*       � �� 5+      �� 5,      " ��6&   .R�5-      �� $ border��6!   #��z��61   $ 5��5.   !   $��0q�5/   !   ._�50      �� $��Center=�6/   $��Center=�6L   51      	 52      P� 53      
 �@J�IsMouseOver$
 True��6=   . �54   "   ��  � �� �#5BC0DE6?   �� 55   "    �@J�Effect 56   &     57   *   $ Black��6;   $ 10Q�6I   $ 1Q�6Y   $	 0.7Q�6i   59   #   �� 5:   "    �@J�Opacity$	 0.9Q�6<   5;      P� 5<      
 � ��$
 True��6;   . �5=   "   ��  � z� 5>   &   �� 5?   *   $
 0.98Q�69   $
 0.98Q�6G   5A   #   5B      P� 5C      
 � {�$ False��6;   . �5D   "   ��  �@J�Opacity$	 0.5Q�6<   5E      5G      5I      5J      �� 5M   
   " ��6)   .�5N      ��  � �� �#2D2D2D6+   �� 5O       � �� �#E0E0E06+   �� 5P       � �� �#3E3E3E6,   �� 5Q       � ��$ 1q�60   �� 5R       � ��$ 5q�6(   5S      5T       � DataContext 5U      8   2AfbeeldingenUitzoeken.ViewModels.SettingsViewModel  5V   
   .��5X      � #��[ 6   .+�5Y   
   �� $��20q�6   .+�5[      �� $2��,Source Library (Uit te zoeken afbeeldingen):��6   #��[ 5\      $��0,0,0,5q�5]      � 5^      $��0,0,0,20q�6    � ColumnDefinitions 5_      �� 5`      $��* �6'   �� 5a      $
��Auto �6'   �� 5b      $
��Auto �6'   .+�5d      ��  �� 
IsReadOnly$
 True��5e      #��[
 5f      $��0��5g      ��5d      �� *LibraryPath+5g      �� 5h      $��1��5i      #��[	 5j      ���#3E3E3E$��40��5l      $��5,0,0,0q�5m       � ToolTip$ Select folder��5n      ��5h      �� *SelectLibraryCommand+.��5o      �� $
��📂��6    $��14(�6*   5p      �� 5q      $	��⚠��6   $��2��5r      #��[ 5s      y�5t      �� *IsLibraryPathInvalid+ �� 	Converter# [  5u      �� 5w      $��Keep Folder:��6   #��[ 5x      $��0,0,0,5q�5y      � 5z      $��0,0,0,20q�6    5{      �� 5|      $��* �6'   �� 5}      $
��Auto �6'   �� 5~      $
��Auto �6'   .+�5�      �� $
 True��5�      #��[
 5�      $��0��5�      ��5�      �� *KeepFolderPath+5�      �� 5�      $��1��5�      #��[	 5�      ���#3E3E3E$��40��5�      $��5,0,0,0q�5�      $ Select folder��5�      ��5�      �� *SelectKeepFolderCommand+.��5�      �� $
��📂��6    $��14(�6*   5�      �� 5�      $	��⚠��6   $��2��5�      #��[ 5�      y�5�      �� *IsKeepFolderPathInvalid+# [  5�      �� 5�      $��Bin Folder:��6   #��[ 5�      $��0,0,0,5q�5�      � 5�      $��0,0,0,20q�6    5�      �� 5�      $��* �6'   �� 5�      $
��Auto �6'   �� 5�      $
��Auto �6'   .+�5�      �� $
 True��5�      #��[
 5�      $��0��5�      ��5�      �� *BinFolderPath+5�      �� 5�      $��1��5�      #��[	 5�      ���#3E3E3E$��40��5�      $��5,0,0,0q�5�      $ Select folder��5�      ��5�      �� *SelectBinFolderCommand+.��5�      �� $
��📂��6    $��14(�6*   5�      �� 5�      $	��⚠��6   $��2��5�      #��[ 5�      y�5�      �� *IsBinFolderPathInvalid+# [  5�      �� 5�      $��Check Later Folder:��6   #��[ 5�      $��0,0,0,5q�5�      � 5�      $��0,0,0,20q�6    5�      �� 5�      $��* �6'   �� 5�      $
��Auto �6'   �� 5�      $
��Auto �6'   .+�5�      �� $
 True��5�      #��[
 5�      $��0��5�      ��5�      �� *CheckLaterFolderPath+5�      �� 5�      $��1��5�      #��[	 5�      ���#3E3E3E$��40��5�      $��5,0,0,0q�5�      $ Select folder��5�      ��5�      �� *SelectCheckLaterFolderCommand+.��5�      �� $
��📂��6    $��14(�6*   5�      �� 5�      $	��⚠��6   $��2��5�      #��[ 5�      y�5�      �� *IsCheckLaterFolderPathInvalid+# [  5�      �� 5�      $
��Save��6   $	��120��5�      $��35��5�      #��[ 5�      $��Right=�5�      $��0,20,0,0q�5�      #��[	 5�      ��5�      �� *SaveCommand+y�5�      �� *IsSaveButtonVisible+# [  �� 5�      #��[ 5�       J� Opacity$	 0.6Q�5�      $��11(�5�      $��Right=�5�      $��0,10,0,0q�5�      ��5�      �� *VersionInfo+5�      5�      5�      5�      