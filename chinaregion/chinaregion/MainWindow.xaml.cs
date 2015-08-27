using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Local;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace chinaregion
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        string[] layerids; ArcGISLocalDynamicMapServiceLayer _arcGISLocalDynamicMapServiceLayer;


        private void btnopen_Click(object sender, RoutedEventArgs e)
        {
            // Disable the UI Grid until the ArcGISDynamicMapServiceLayer has initialized.
           // DynamicLayersUiGrid.IsEnabled = false;

            // Create a new local map service from an MPK and enable the 
            // Dynamic Layers capability
           

            // Start the local map service and once initialized
           

            // Setup the OpenFiledialog.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mpk (*.mpk)|*.mpk";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    List<string> fileNames = new List<string>();
                    string filename = openFileDialog.FileName;
                    LocalMapService localMapService = new LocalMapService()
                    {
                        Path = filename,
                        EnableDynamicLayers = true
                    };
                    localMapService.StartAsync((mapService) =>
                    {
                        // create a new local dynamic map service layer based on the local map service 
                        _arcGISLocalDynamicMapServiceLayer = new ArcGISLocalDynamicMapServiceLayer(localMapService)
                        {
                            ID = "_arcGISLocalDynamicMapServiceLayer",
                            ImageFormat = ArcGISDynamicMapServiceLayer.RestImageFormat.PNG32,
                        };

                        // Set up an event handler for the layer initialized event
                        _arcGISLocalDynamicMapServiceLayer.Initialized += (s, re) =>
                        {
                            //Enable the UI once the layer has initialized
                         //   DynamicLayersUiGrid.IsEnabled = true;
                            DataContext = this;
                           // IsBusy = false;
                        };

                        // Add the layer to the map
                        MyMap.Layers.Add(_arcGISLocalDynamicMapServiceLayer);
                    });
                 //   ArcGISLocalDynamicMapServiceLayer layer = new ArcGISLocalDynamicMapServiceLayer() { Path = filename, ID = "_arcGISLocalDynamicMapServiceLayer" };
                  //  _arcGISLocalDynamicMapServiceLayer = layer;
                    layerids = new string[1] { "_arcGISLocalDynamicMapServiceLayer" };
                    legendtoolkit.LayerIDs = layerids;
                 //   MyMap.Layers.Add(layer);
                  //  openlayer.Path = filename;
                    //foreach (var item in openFileDialog.FileName)
                    //{
                    //    fileNames.Add(Path.GetFileNameWithoutExtension(item));
                    //}
                    //// Call the add dataset method with workspace type, parent directory path, file names (without extensions) and delegate.
                    //AddFileDatasetToDynamicMapServiceLayer(WorkspaceFactoryType.Shapefile,
                    //    Path.GetDirectoryName(openFileDialog.FileName),
                    //    fileNames,
                    //    arcGisLocalDynamicMapServiceLayer =>
                    //    {
                    //        // Add the dynamic map service layer to the map.                           
                    //        MyMap.Layers.Add(arcGisLocalDynamicMapServiceLayer);
                    //    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

        }

        private void Legend_Refreshed(object sender, ESRI.ArcGIS.Client.Toolkit.Legend.RefreshedEventArgs e)
        {
            LayerItemViewModel removeLayerItemVM = null;
          
            if (e.LayerItem.LayerItems != null)
            {
                foreach (LayerItemViewModel layerItemVM in e.LayerItem.LayerItems)
                {
                    if (layerItemVM.IsExpanded)
                        layerItemVM.IsExpanded = false;

                    if (layerItemVM.Label == "states")
                        removeLayerItemVM = layerItemVM;
                }

                if (removeLayerItemVM != null)
                    e.LayerItem.LayerItems.Remove(removeLayerItemVM);
            }
            else
            {
                e.LayerItem.IsExpanded = false;
            }

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
        // Member variables representing the LayerDrawingOptions definition for each layer.
        LayerDrawingOptions _citiesLayerDrawingOptions;
        LayerDrawingOptions _highwaysLayerDrawingOptions;
        LayerDrawingOptions _statesLayerDrawingOptions;
        // Member variable for the random number generator to create symbol sizes, line widths, etc.
        Random _random = new Random();


        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If there is already an instance of the Cities LayerDrawOptions definition remove it from the layer's LayerDrawingOptions collection.
            if (_citiesLayerDrawingOptions != null)
                ResetRenderer(_citiesLayerDrawingOptions, false);

            // Create a new Layer Drawing Options object which will define the new renderer for a specific layer in the underlying local map service
            _citiesLayerDrawingOptions = new LayerDrawingOptions();

            // Set the ID of the map layer in the local map service
            _citiesLayerDrawingOptions.LayerID =0;

            // Create a new simple renderer with a simple marker symbol (for feature layers of point geometry type)
            _citiesLayerDrawingOptions.Renderer = new SimpleRenderer()
            {
                Symbol = new SimpleMarkerSymbol()
                {
                    Color = new SolidColorBrush(GetRandomColor()),
                    Size = _random.Next(4, 12),
                    Style = GetRandomEnumValue<SimpleMarkerSymbol.SimpleMarkerStyle>()
                }
            };

            // Create a new layer drawing options collection and set the layer drawing options property on the local dynamic map service layer
            if (_arcGISLocalDynamicMapServiceLayer.LayerDrawingOptions == null)
                _arcGISLocalDynamicMapServiceLayer.LayerDrawingOptions = new LayerDrawingOptionsCollection() { _citiesLayerDrawingOptions };
            else
                _arcGISLocalDynamicMapServiceLayer.LayerDrawingOptions.Add(_citiesLayerDrawingOptions);

            // Refresh the layer
            _arcGISLocalDynamicMapServiceLayer.Refresh();

        }
        public void ResetRenderer(LayerDrawingOptions layerDrawingOptions, bool refreshMap)
        {
            // Exit if the layer has no LayerDrawingOptions
            if (_arcGISLocalDynamicMapServiceLayer.LayerDrawingOptions == null || _arcGISLocalDynamicMapServiceLayer.LayerDrawingOptions.Count == 0)
                return;

            // Remove the LayerDrawingOptions from the layer's LayerDrawingOptions collection
            if (_arcGISLocalDynamicMapServiceLayer.LayerDrawingOptions.Contains(layerDrawingOptions))
            {
                _arcGISLocalDynamicMapServiceLayer.LayerDrawingOptions.Remove(layerDrawingOptions);

                if (refreshMap)
                    _arcGISLocalDynamicMapServiceLayer.Refresh();
            }
        }

        // Utility function: Generate a random System.Windows.Media.Color
        private Color GetRandomColor()
        {
            var colorBytes = new byte[3];
            _random.NextBytes(colorBytes);
            Color randomColor = Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
            return randomColor;
        }

        // Utility function: Generate a random value from the specified enumeration type
        public T GetRandomEnumValue<T>()
        {
            T[] values = (T[])Enum.GetValues(typeof(T));
            return values[new Random().Next(0, values.Length)];
        }

    }
}
