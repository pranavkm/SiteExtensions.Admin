// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;

namespace SiteExtensions.Administration
{
    public class WebProjectManager
    {
        private static readonly Uri _remoteSource = new Uri("http://siteextensions.azurewebsites.net/api/v2/");
        private readonly string _siteRoot;
        private readonly IPackageRepository _sourceRepository;
        private readonly IPackageRepository _localRepository;

        public WebProjectManager()
        {
            _siteRoot = GetSiteRoot();
            _sourceRepository = new DataServicePackageRepository(_remoteSource);
            string localRepoDir = Path.Combine(_siteRoot, "SiteExtensions"); 
            _localRepository = new LocalPackageRepository(localRepoDir);
        }

        public IPackageRepository SourceRepository
        {
            get { return _sourceRepository; }
        }

        public IPackageRepository LocalRepository
        {
            get { return _localRepository; }
        }

        private static string GetSiteRoot()
        {
            string path = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\home");
            if (Directory.Exists(path))
            {
                return path;
            }

            return Environment.ExpandEnvironmentVariables(@"%HOME%");
        }

        public virtual IQueryable<IPackage> GetRemotePackages(string searchTerms)
        {
            if (String.IsNullOrEmpty(searchTerms))
            {
                return _sourceRepository.GetPackages().OrderByDescending(f => f.DownloadCount);
            }

            return _sourceRepository.Search(searchTerms, allowPrereleaseVersions: false);
        }

        public IQueryable<IPackage> GetInstalledPackages(string searchTerms)
        {
            return _localRepository.Search(searchTerms, allowPrereleaseVersions: true);
        }

        public IEnumerable<IPackage> GetPackagesWithUpdates(string searchTerms)
        {
            var packagesToUpdate = GetInstalledPackages(searchTerms).ToList();
            return _sourceRepository.GetUpdates(packagesToUpdate, includePrerelease: false, includeAllVersions: false);
        }

        public void InstallPackage(IPackage package)
        {
            var directoryToExpandTo = Path.Combine(_localRepository.Source, package.Id);
            foreach (var file in package.GetContentFiles())
            {
                var fullPath = Path.Combine(directoryToExpandTo, file.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                using (Stream writeStream = File.OpenWrite(fullPath),
                              readStream = file.GetStream())
                {
                    readStream.CopyTo(writeStream);
                }
            }
        }

        public void UpdatePackage(IPackage package)
        {
            UninstallPackage(package);
            InstallPackage(package);
        }

        public void UninstallPackage(IPackage package)
        {
            var directoryToExpandTo = Path.Combine(_localRepository.Source, package.Id);
            Directory.Delete(directoryToExpandTo, recursive: true);
        }

        public bool IsPackageInstalled(IPackage package)
        {
            return _localRepository.Exists(package);
        }

        public IPackage GetUpdate(IPackage package)
        {
            return _sourceRepository.GetUpdates(new[] { package }, includePrerelease: false, includeAllVersions: false).SingleOrDefault();
        }
    }
}
