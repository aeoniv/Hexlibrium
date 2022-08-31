using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;

namespace HEXLIBRIUM
{
    public class AppManager : MonoBehaviour
    {
        
        
        [SerializeField] private SelectionPanel selectionPanel;

        private void OnEnable()
        {
            selectionPanel.UploadButtonPressed += UploadToIpfs;
            
        }

        private void OnDisable()
        {
            selectionPanel.UploadButtonPressed -= UploadToIpfs;
        }


        private async void UploadToIpfs(string imgName, string imgDesc, string imgPath, byte[] imgData)
        {
            Debug.Log("Test");

            string ipfsImagePath = await SaveImageToIpfs(imgName, imgData);
            
            Debug.Log(ipfsImagePath);

            // Build Metadata
            object metadata = BuildMetadata(imgName, imgDesc, ipfsImagePath);
            string metadataName = $"{imgName}.json";
            // Store metadata to IPFS
            string json = JsonConvert.SerializeObject(metadata);
            string base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            
            string ipfsMetadataPath = await SaveToIpfs(metadataName, base64Data);
            
            Debug.Log(ipfsMetadataPath);
            
            selectionPanel.ResetUploadButton();
            
        }

        private async UniTask<string> SaveToIpfs(string name, string data)
        {
            string pinPath = null;

            try
            {
                IpfsFileRequest request = new IpfsFileRequest()
                {
                    Path = name,
                    Content = data
                };
                
                List<IpfsFileRequest> requests = new List<IpfsFileRequest> {request};
                List<IpfsFile> resp = await Moralis.GetClient().Web3Api.Storage.UploadFolder(requests);
                
                IpfsFile ipfs = resp.FirstOrDefault<IpfsFile>();
                
                if (ipfs != null)
                {
                    pinPath = ipfs.Path;
                }
            }

            catch (Exception exp)
            {
                Debug.LogError($"IPFS Save failed: {exp.Message}");
            }

            return pinPath;
        }

        private async UniTask<string> SaveImageToIpfs(string name, byte[] imageData)
        {
            return await SaveToIpfs(name, Convert.ToBase64String(imageData));
        }

        private static object BuildMetadata(string name, string desc, string imageUrl)
        {
            object obj = new { name = name, description = desc, image = imageUrl };

            return obj; 
        }


    }
}